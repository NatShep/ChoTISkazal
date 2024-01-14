using System.Threading.Tasks;
using SayWhat.Bll.Services;

namespace Chotiskazal.Bot.CommandHandlers;

public class SettingsEnableNotifications : IBotCommandHandler
{
    private readonly UserService _userService;

    public SettingsEnableNotifications(UserService userService)
    {
        _userService = userService;
    }

    public bool Acceptable(string text) =>
        text == BotCommands.NotificationSettingsOn || text == BotCommands.NotificationSettingsOff;

    public string ParseArgument(string text) => text == BotCommands.NotificationSettingsOn ? "true" : "false";

    public async Task Execute(string argument, ChatRoom chat)
    {
        var enableNotification = argument == "true";
        chat.User.NotificationState.NotificationEnabled = enableNotification;
        chat.User.OnAnyActivity();
        await _userService.Update(chat.User);
        await chat.SendMessageAsync(chat.Texts.SettingsApplied, InlineButtons.MainMenu(chat.Texts));
    }
}