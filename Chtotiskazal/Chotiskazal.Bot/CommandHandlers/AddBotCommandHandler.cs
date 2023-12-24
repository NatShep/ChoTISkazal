using System.Threading.Tasks;
using Chotiskazal.Bot.ChatFlows.FlowTranslation;
using Chotiskazal.Bot.Hooks;
using SayWhat.Bll.Services;

namespace Chotiskazal.Bot.CommandHandlers;

public class AddBotCommandHandler : IBotCommandHandler {
    private readonly AddWordService _addWordsService;
    private readonly ButtonCallbackDataService _buttonCallbackDataService;
    private readonly TranslationSelectedUpdateHook _translationSelectedUpdateHook;

    public AddBotCommandHandler(
        AddWordService addWordsService, ButtonCallbackDataService buttonCallbackDataService,
        TranslationSelectedUpdateHook translationSelectedUpdateHook) {
        _addWordsService = addWordsService;
        _buttonCallbackDataService = buttonCallbackDataService;
        _translationSelectedUpdateHook = translationSelectedUpdateHook;
    }


    public bool Acceptable(string text) => text == BotCommands.Translate;
    public string ParseArgument(string text) => null;

    public Task Execute(string argument, ChatRoom chat) =>
        new TranslateFlow(chat, _addWordsService, _buttonCallbackDataService, _translationSelectedUpdateHook)
            .Enter(argument);
}