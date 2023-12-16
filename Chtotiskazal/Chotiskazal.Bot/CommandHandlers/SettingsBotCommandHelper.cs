using System.Threading.Tasks;
using Chotiskazal.Bot.ChatFlows;
using SayWhat.Bll.Services;

namespace Chotiskazal.Bot.CommandHandlers;

public class SettingsBotCommandHelper : IBotCommandHandler {
    private readonly UserService _userService;

    public SettingsBotCommandHelper(UserService userService) {
        _userService = userService;
    }

    public bool Acceptable(string text) => text == BotCommands.Settings;
    public string ParseArgument(string text) => null;

    public Task Execute(string argument, ChatRoom chat) => new SettingsFlow(chat, _userService).EnterAsync();
}