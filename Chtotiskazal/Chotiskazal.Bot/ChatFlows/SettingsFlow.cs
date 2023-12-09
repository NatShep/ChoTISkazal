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
            new InlineKeyboardButton
            {
                CallbackData = RemindInlineData,  Text = Chat.Texts.RemindSettingsButton
            },
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
            new InlineKeyboardButton
            {
                CallbackData = "1", Text = Chat.Texts.RemindEveryDay
            },
            new InlineKeyboardButton
            {
                CallbackData = "3", Text = Chat.Texts.RemindEveryThreeDays
            },
            new InlineKeyboardButton
            {
                CallbackData = "7", Text = Chat.Texts.RemindEveryWeek
            },
            new InlineKeyboardButton
            {
                CallbackData = "-1", Text = Chat.Texts.DoNotRemind
            },
            new InlineKeyboardButton
            {
                CallbackData = BotCommands.Start, Text = Chat.Texts.CancelButton
            }
        );

        user.SetRemindFrequency(int.Parse(await Chat.WaitInlineKeyboardInput()));
        await _userService.Update(user);
        await Chat.SendMessageAsync(Chat.Texts.SettingsApplied, InlineButtons.MainMenu(Chat.Texts));
    }
}