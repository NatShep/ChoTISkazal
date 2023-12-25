#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Hooks;
using SayWhat.Bll;
using SayWhat.Bll.Dto;
using SayWhat.Bll.Services;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ChatFlows.FlowTranslation;

internal class TranslateFlow {
    private ChatRoom Chat { get; }
    private readonly AddWordService _addWordService;
    private readonly ButtonCallbackDataService _buttonCallbackDataService;
    private readonly TranslationSelectedUpdateHook _translationSelectedUpdateHook;

    public TranslateFlow(
        ChatRoom chat,
        AddWordService addWordService,
        ButtonCallbackDataService buttonCallbackDataService,
        TranslationSelectedUpdateHook translationUpdateHookHandler
    ) {
        Chat = chat;
        _addWordService = addWordService;
        _buttonCallbackDataService = buttonCallbackDataService;
        _translationSelectedUpdateHook = translationUpdateHookHandler;
    }

    public async Task Enter(string? word = null) {
        do {
            word = await EnterSingleWordAsync(word);
        } while (!string.IsNullOrWhiteSpace(word));
    }

    private async Task<string?> EnterSingleWordAsync(string? input = null) {
        if (string.IsNullOrWhiteSpace(input) || input.StartsWith("/")) {
            await Chat.SendMessageAsync($"{Emojis.Translate} {Chat.Texts.EnterWordOrStart}");
            input = await Chat.WaitUserTextInputAsync();
        }

        Chat.User.OnAnyActivity();

        // Search translations in local dictionary
        UserWordModel? alreadyExistUserWord = null;
        var isRussian = input.IsRussian();
        // Search word in UserWord Table for English Word
        if (!isRussian)
            alreadyExistUserWord = await _addWordService.GetWordNullByEngWord(Chat.User, input);

        // Search or get translation for word
        var translations = await _addWordService.FindInLocalDictionaryWithExamples(input);
        if (!translations.Any()) // if not, search it in Ya dictionary
            translations = await _addWordService.TranslateWordAndAddToDictionary(input);

        if (translations?.Any() != true) {
            // Translations not found. So try to translate something:
            var smartTranslation = await _addWordService.SmartTranslate(input);
            if (smartTranslation == null) {
                // There is definitely no translations for user's bullshit input (or may be gTranslate is broken)
                await Chat.SendMessageAsync(Chat.Texts.NoTranslationsFound);
                Reporter.ReportTranslationNotFound(Chat.User.TelegramId);
                return null;
            }

            // it is single "smart" translation
            translations = new List<Translation> { smartTranslation };
        }

        LastWordTranslationHandler? handler = null;

        var firstTranslation = translations[0];
        if (translations.Count == 1 && firstTranslation.WordType == UserWordType.Phrase) {
            // it is a long phrase. If it is not very long, and not added yet - than save it
            if (firstTranslation.CanBeSavedToDictionary && alreadyExistUserWord == null)
                await _addWordService.AddTranslationToUser(Chat.User, firstTranslation.GetEnRu());

            await Chat.SendMarkdownMessageAsync(
                Chat.Texts.HereIsThePhraseTranslation(firstTranslation.TranslatedText),
                TranslateWordHelper.GetTranslateMenuButtons(Chat.Texts));
        }
        else {
            // Simple word translation handler
            // get button selection marks. It works only for english words!
            bool[] selectionMarks = GetSelectionMarks(translations, alreadyExistUserWord);

            if (!selectionMarks[0]) {
                // Automatically select first translation if it was not selected before
                await _addWordService.AddTranslationToUser(Chat.User, translations[0].GetEnRu());
                selectionMarks[0] = true;
            }

            // getting first transcription
            var transcription = translations.FirstOrDefault(t =>
                !string.IsNullOrWhiteSpace(t.EnTranscription))?.EnTranscription;
            handler = new LastWordTranslationHandler(
                translations, Chat, _addWordService, _buttonCallbackDataService, selectionMarks);
            _translationSelectedUpdateHook.SetLastTranslationHandler(handler);
            await handler.SendTranslationMessage(Chat.Texts.HereAreTranslations(input, transcription));
        }

        Reporter.ReportTranslationRequsted(Chat.User.TelegramId, input.IsRussian());

        if (input.IsRussian()) Chat.User.OnRussianWordTranslationRequest();
        else Chat.User.OnEnglishWordTranslationRequest();

        try {
            // user request next word
            var res = await Chat.WaitUserTextInputAsync();
            // notify handler, that we leave the flow
            return res;
        }
        finally {
            var _ = handler?.OnNextTranslationRequest();
        }
    }

    private bool[] GetSelectionMarks(
        IReadOnlyList<Translation> translations, UserWordModel? alreadyExistUserWord) {
        bool[] ans = new bool[translations.Count];
        if (alreadyExistUserWord == null)
            return ans;
        for (int i = 0; i < translations.Count; i++)
            ans[i] = alreadyExistUserWord.HasTranslation(translations[i].TranslatedText);
        return ans;
    }
}