#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SayWhat.Bll.Services;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.ChatFlows.FlowTranslation;

public class LastWordTranslationHandler : LastTranslationHandlerBase {
    private readonly IReadOnlyList<SayWhat.Bll.Dto.Translation> _translations;

    public LastWordTranslationHandler(
        IReadOnlyList<SayWhat.Bll.Dto.Translation> translations, ChatRoom chat,
        AddWordService addWordService, ButtonCallbackDataService buttonCallbackDataService,
        bool[] selectionMarks) :
        base(translations.FirstOrDefault()?.OriginText, chat, addWordService, buttonCallbackDataService) {
        _translations = translations;
        AreSelected = selectionMarks;
    }

    protected override async Task<IList<InlineKeyboardButton[]>> CreateCustomButtons() {
        var buttons = new List<InlineKeyboardButton[]>();
        var i = 0;
        foreach (var translation in _translations) {
            var button = await TranslateWordHelper.CreateButtonFor(
                buttonCallbackDataService: ButtonCallbackDataService,
                text: translation.TranslatedText,
                translation: translation,
                selected: AreSelected[i]);

            buttons.Add(new[] { button });
            i++;
        }

        return buttons;
    }

    public override async Task HandleButtonClick(Update update, TranslationButtonData buttonData) {
        if (OriginWordText.Equals(buttonData.Origin)) {
            // if translation is cached - fall into handler for fast handling
            await HandleLocal(buttonData.Translation, update);
            return;
        }

        // word is not cached
        // so we need to find already translated items
        var allTranslations = await AddWordService.FindInLocalDictionaryWithExamples(buttonData.Origin);
        var originMessageButtons = update.CallbackQuery
            .Message
            ?.ReplyMarkup
            ?.InlineKeyboard
            ?.SelectMany(i => i)
            .ToArray();

        if (originMessageButtons == null) {
            await Chat.ConfirmCallback(update.CallbackQuery.Id);
            return;
        }

        if (originMessageButtons.Length < allTranslations.Count) {
            // Такое может произойти если  в оригинальном сообщении была слишком длинная кнопка (и она вырезана)
            // Или что то поменялось в бд.
            // Это редкий случай поэтому особая обработка не делается
            await Chat.ConfirmCallback(update.CallbackQuery.Id);
            return;
        }

        var selectionMarks = await GetSelectionMarks(ButtonCallbackDataService, allTranslations, originMessageButtons);

        var index = TranslateWordHelper.FindIndexOf(allTranslations, buttonData.Translation);
        if (index == -1) {
            await Chat.ConfirmCallback(update.CallbackQuery.Id);
            return;
        }

        var selectedBefore = selectionMarks[index];
        if (!selectedBefore) {
            selectionMarks[index] = true;
            await AddWordService.AddTranslationToUser(Chat.User, allTranslations[index].GetEnRu());
            await Chat.AnswerCallbackQueryWithTooltip(update.CallbackQuery.Id,
                Chat.Texts.MessageAfterTranslationIsSelected(allTranslations[index]));
        }

        else {
            selectionMarks[index] = false;
            await AddWordService.RemoveTranslationFromUser(Chat.User, allTranslations[index].GetEnRu());
            await Chat.AnswerCallbackQueryWithTooltip(update.CallbackQuery.Id,
                Chat.Texts.MessageAfterTranslationIsDeselected(allTranslations[index]));
        }

        var buttons = new List<InlineKeyboardButton[]>();
        foreach (var translation in allTranslations) {
            var translationIndex = TranslateWordHelper.FindIndexOf(allTranslations, translation.TranslatedText);
            var button = await TranslateWordHelper.CreateButtonFor(
                ButtonCallbackDataService, translation.TranslatedText, translation, selectionMarks[translationIndex]);
            buttons.Add(new[] { button });
        }

        await Chat.EditMessageButtons(
            update.CallbackQuery.Message.MessageId,
            buttons.ToArray());
    }


    private async Task HandleLocal(string translation, Update update) {
        var index = TranslateWordHelper.FindIndexOf(_translations, translation);
        if (index == -1)
            return;

        AreSelected[index] = !AreSelected[index];
        await HandleSelection(AreSelected[index], _translations[index], update.CallbackQuery.Message.MessageId);

        var message = "";

        if (AreSelected[index])
            message = Chat.Texts.MessageAfterTranslationIsSelected(_translations[index]);
        else
            message = Chat.Texts.MessageAfterTranslationIsDeselected(_translations[index]);

        await Chat.AnswerCallbackQueryWithTooltip(update.CallbackQuery.Id, message);
    }

    private static async Task<bool[]> GetSelectionMarks(ButtonCallbackDataService buttonCallbackDataService,
        IReadOnlyList<SayWhat.Bll.Dto.Translation> allTranslations, InlineKeyboardButton[] originMessageButtons) {
        bool[] selectionMarks = new bool[allTranslations.Count];
        int i = 0;
        foreach (var originMessageButton in originMessageButtons) {
            var data = await buttonCallbackDataService.GetButtonDataOrNull(originMessageButton.CallbackData);
            if (data != null) {
                if (allTranslations[i].TranslatedText.Equals(data.Translation) && data.IsSelected)
                    selectionMarks[i] = true;
            }

            i++;
        }

        return selectionMarks;
    }
}