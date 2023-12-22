using System.Threading.Tasks;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Users;
using Telegram.Bot.Types.ReplyMarkups;

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

    private async Task SetRemindFrequency(UserModel user) {
        await Chat.SendMessageAsync(
            Chat.Texts.RemindSettingsMessage,
            InlineButtons.Button(Chat.Texts.RemindEveryDay, "1"),
            InlineButtons.Button(Chat.Texts.RemindEveryThreeDays, "3"),
            InlineButtons.Button(Chat.Texts.RemindEveryWeek, "7"),
            InlineButtons.Button(Chat.Texts.DoNotRemind, "-1"),
            InlineButtons.Button(Chat.Texts.CancelButton, BotCommands.Start)
        );

        user.SetRemindFrequency(int.Parse(await Chat.WaitInlineKeyboardInput()));
        await _userService.Update(user);
        await Chat.SendMessageAsync(Chat.Texts.SettingsApplied, InlineButtons.MainMenu(Chat.Texts));
    }
}