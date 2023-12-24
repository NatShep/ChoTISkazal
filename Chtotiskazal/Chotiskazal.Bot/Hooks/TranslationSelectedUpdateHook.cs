using System.Threading.Tasks;
using Chotiskazal.Bot.ChatFlows.FlowTranslation;
using SayWhat.Bll.Services;
using Telegram.Bot.Types;

namespace Chotiskazal.Bot.Hooks;

public class TranslationSelectedUpdateHook : IChatUpdateHook
{
    private ChatRoom Chat { get; }
    private readonly AddWordService _addWordService;
    private readonly ButtonCallbackDataService _buttonCallbackDataService;
        
    public TranslationSelectedUpdateHook(
        ChatRoom chat,
        AddWordService addWordService,
        ButtonCallbackDataService buttonCallbackDataService)
    {
        Chat = chat;
        _addWordService = addWordService;
        _buttonCallbackDataService = buttonCallbackDataService;
    }

    public void SetLastTranslationHandler(LastTranslationHandlerBase handler) => _cachedHandlerTranslationOrNull = handler;
        
    private LastTranslationHandlerBase _cachedHandlerTranslationOrNull = null;
        
    public bool CanBeHandled(Update update) => 
        update.CallbackQuery?.Data!=null 
        && (update.CallbackQuery.Data.StartsWith(ButtonCallbackDataService.TranslationDataPrefix)
            ||
            update.CallbackQuery.Data.StartsWith(ButtonCallbackDataService.TranslationDataPrefixForLargeSize));

    public async Task Handle(Update update)
    {
        var buttonData = await _buttonCallbackDataService.GetButtonDataOrNull(update.CallbackQuery.Data);
        if (buttonData == null)
        {
            await Chat.ConfirmCallback(update.CallbackQuery.Id);
            return;
        }
        // last translation is cached. 
        await _cachedHandlerTranslationOrNull.HandleButtonClick(update, buttonData);
    }
}