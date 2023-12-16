using System.Threading.Tasks;
using Chotiskazal.Bot.ChatFlows;
using Chotiskazal.Bot.Hooks;
using SayWhat.Bll.Services;

namespace Chotiskazal.Bot.CommandHandlers;

public class ShowWellLearnedWordsCommandHandler : IBotCommandHandler {
    private readonly UsersWordsService _userWordsService;
    private readonly LeafWellKnownWordsUpdateHook _wellKnownWordsUpdateHook;

    public ShowWellLearnedWordsCommandHandler(UsersWordsService userWordsService,
        LeafWellKnownWordsUpdateHook wellKnownWordsUpdateHook) {
        _userWordsService = userWordsService;
        _wellKnownWordsUpdateHook = wellKnownWordsUpdateHook;
    }

    public bool Acceptable(string text) => text == BotCommands.Words;
    public string ParseArgument(string text) => null;

    public Task Execute(string argument, ChatRoom chat) => new ShowWellKnownWordsFlow(
            chat, _userWordsService, _wellKnownWordsUpdateHook)
        .EnterAsync();
}