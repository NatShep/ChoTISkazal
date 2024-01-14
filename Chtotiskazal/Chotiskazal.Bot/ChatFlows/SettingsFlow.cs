using System.Threading.Tasks;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Users;

namespace Chotiskazal.Bot.ChatFlows;

public class SettingsFlow {
    private const string RemindInlineData = "remind";

    private readonly UserService _userService;

    public SettingsFlow(
        ChatRoom chat,
        UserService userService) {
        Chat = chat;
        _userService = userService;
    }

    private ChatRoom Chat { get; }

    public async Task EnterAsync() {
        await Chat.SendMessageAsync(Chat.Texts.AllowedSettings,
            InlineButtons.Button(Chat.Texts.RemindSettingsButton, RemindInlineData),
            InlineButtons.Chlang(Chat.Texts),
            InlineButtons.MainMenu(Chat.Texts)
        );

        var chosenSetting = await Chat.WaitInlineKeyboardInput();

        switch (chosenSetting) {
            case RemindInlineData:
                await SetRemindFrequency(Chat.User);
                break;
            default:
                return;
        }
    }

    private async Task SetRemindFrequency(UserModel user)
    {
        var remindButton = user.NotificationState.NotificationEnabled
            ? InlineButtons.Button(Chat.Texts.TurnOffRemind, BotCommands.NotificationSettingsOff)
            : InlineButtons.Button(Chat.Texts.TurnOnRemind, BotCommands.NotificationSettingsOn);
        
        await Chat.SendMessageAsync(
            Chat.Texts.RemindSettingsMessage,
            remindButton,
            InlineButtons.Button(Chat.Texts.CancelButton, BotCommands.Start)
        );
    }
}