using System.Collections.Generic;
using System.Threading.Tasks;
using SayWhat.Bll.Services;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.ChatFlows.FlowTranslation;

public class LastPhraseTranslationHandler : LastTranslationHandlerBase {
    private readonly SayWhat.Bll.Dto.Translation _phraseTranslation;
    private bool _isSelected;

    public LastPhraseTranslationHandler(
        SayWhat.Bll.Dto.Translation phraseTranslation, ChatRoom chat,
        AddWordService addWordService, ButtonCallbackDataService buttonCallbackDataService, bool isSelected) : base(
        phraseTranslation.OriginText,
        chat,
        addWordService,
        buttonCallbackDataService) {
        _phraseTranslation = phraseTranslation;
        _isSelected = isSelected;
    }

    protected override async Task<IList<InlineKeyboardButton[]>> CreateCustomButtons() {
        var theButton = await TranslateWordHelper.CreateButtonFor(
            buttonCallbackDataService: ButtonCallbackDataService,
            text: Chat.Texts.ToLearnPhrase,
            translation: _phraseTranslation,
            selected: _isSelected
        );
        return new[] { new[] { theButton } };
    }

    public override async Task HandleButtonClick(Update update, TranslationButtonData buttonData) {
        if (OriginWordText.Equals(buttonData.Origin)) {
            _isSelected = !_isSelected;
            await HandleSelection(_isSelected, _phraseTranslation, update.CallbackQuery.Message.MessageId);
        }
        else {
            // Do nothing if it is deselected and user remove mark. We don't save phrases.
            await Chat.AnswerCallbackQueryWithTooltip(update.CallbackQuery.Id, Chat.Texts.ItWasLongTimeAgo);
        }
    }
}