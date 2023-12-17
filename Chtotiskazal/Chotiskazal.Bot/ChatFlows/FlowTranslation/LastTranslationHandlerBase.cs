using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.Bll.Services;
using SayWhat.Bll.Strings;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.ChatFlows.FlowTranslation;

public abstract class LastTranslationHandlerBase {
    protected ChatRoom Chat { get; }
    protected string OriginWordText { get; }

    protected readonly AddWordService AddWordService;
    protected readonly ButtonCallbackDataService ButtonCallbackDataService;
    private bool _isLastMessageInTheChat = true;

    /// <summary>
    /// Origin message of translation
    /// </summary>
    private int? _originMessageId;

    protected bool[] AreSelected;

    protected LastTranslationHandlerBase(
        string originText,
        ChatRoom chat,
        AddWordService addWordService,
        ButtonCallbackDataService buttonCallbackDataService
        ) {
        Chat = chat;
        OriginWordText = originText;
        AddWordService = addWordService;
        ButtonCallbackDataService = buttonCallbackDataService;
    }

    /// <summary>
    /// User make next request
    /// </summary>
    public async Task OnNextTranslationRequest() {
        _isLastMessageInTheChat = false;
        if (_originMessageId != null) {
            //hide "menu" and "translation" buttons
            var buttons = await CreateButtons();
            var _ = Chat.EditMessageButtons(_originMessageId.Value, buttons.ToArray());
        }
    }
    
    protected abstract Task<IList<InlineKeyboardButton[]>> CreateCustomButtons();
    
    public async Task SendTranslationMessage(Markdown message) {
        var buttons = (await CreateButtons()).ToArray();
        if (!buttons.Any()) {
            await Chat.SendMessageAsync(Chat.Texts.NoTranslationsFound);
            Reporter.ReportTranslationNotFound(Chat.User.TelegramId);
            return;
        }

        _originMessageId = await Chat.SendMarkdownMessageAsync(message , buttons);
    }  
    
    public abstract Task HandleButtonClick(Update update, TranslationButtonData buttonData);

    protected async Task HandleSelection(bool isSelected, SayWhat.Bll.Dto.Translation translation, int messageId) {
        if (isSelected) //if become selected
        {
            await AddWordService.AddTranslationToUser(Chat.User, translation.GetEnRu());
            Reporter.ReportTranslationPairSelected(Chat.User.TelegramId);
        }
        else {
            await AddWordService.RemoveTranslationFromUser(Chat.User, translation.GetEnRu());
            Reporter.ReportTranslationPairRemoved(Chat.User.TelegramId);
        }

        var buttons = await CreateButtons();
        await Chat.EditMessageButtons(messageId, buttons.ToArray());
    }
    
    private async Task<IList<InlineKeyboardButton[]>> CreateButtons() {
        var buttons = (await CreateCustomButtons()).ToList();
        if (_isLastMessageInTheChat)
            buttons.Add(TranslateWordHelper.GetTranslateMenuButtons(Chat.Texts));
        return buttons;
    }
}