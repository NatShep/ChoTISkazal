using System.Threading.Tasks;
using SayWhat.Bll.Services;

namespace Chotiskazal.Bot.CommandHandlers;

public class ChlangBotCommandHandler : IBotCommandHandler {
    private readonly UserService _userService;
    public ChlangBotCommandHandler(UserService userService) => _userService = userService;
    public bool Acceptable(string text) => text == BotCommands.Chlang;
    public string ParseArgument(string text) => null;

    public async Task Execute(string argument, ChatRoom chat) {
        chat.User.IsEnglishInterface = !chat.User.IsEnglishInterface;
        await _userService.Update(chat.User);
        await chat.SendMessageAsync(chat.Texts.InterfaceLanguageSetuped);
    }
}