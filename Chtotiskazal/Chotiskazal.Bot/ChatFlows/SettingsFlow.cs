using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chotiskazal.Bot.Hooks;
using Chotiskazal.Bot.Interface;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Users;
using SayWhat.MongoDAL.Words;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.ChatFlows {

public class SettingsHelper {
    public const string PrevData = "/wk<<";
    public const string NextData = "/wk>>";

    public static InlineKeyboardButton[] GetPagingKeys() => new[] {
        new InlineKeyboardButton { CallbackData = PrevData, Text = "<<" },
        new InlineKeyboardButton { CallbackData = NextData, Text = ">>" },
    };
}

public class SettingsFlow {
    private readonly UserService _userService;

    public SettingsFlow(
        ChatRoom chat,
        UserService userService) {
        Chat = chat;
        _userService = userService;
    }

    private ChatRoom Chat { get; }

    public async Task EnterAsync() {
        var user = Chat.User;

        var settingMessage = Markdown.Escaped("Доступные настройки:");
        
        await Chat.SendMarkdownMessageAsync(settingMessage, new[]
        {
            
            new[]
            {
                new InlineKeyboardButton
                {
                    CallbackData = "remind",
                    Text = Chat.Texts.RemindSettingsButton
                },
            },
            new[]
            {
                new InlineKeyboardButton
                {
                    CallbackData = BotCommands.Start,
                    Text = Chat.Texts.CancelButton,
                }
            }
        });
        
        var chosenSetting = await Chat.WaitInlineKeyboardInput();

        switch (chosenSetting) {
            case "remind":
                await SetRemindFrequency(user);
                break;
            default:
                return;
        }
    }

    private async Task SetRemindFrequency(UserModel user) {
        await Chat.SendMarkdownMessageAsync(
            Markdown.Escaped(Chat.Texts.RemindSettingsMessage),
            new[]
            {
                new[]
                {
                    new InlineKeyboardButton
                    {
                        CallbackData = "1",
                        Text = Chat.Texts.RemindEveryDay
                    }
                },
                new[]
                {
                    new InlineKeyboardButton
                    {
                        CallbackData = "3",
                        Text = Chat.Texts.RemindEveryThreeDays
                    }
                },
                new[]
                {
                    new InlineKeyboardButton
                    {
                        CallbackData = "7",
                        Text = Chat.Texts.RemindEveryWeek
                    }
                },
                new[]
                {
                    new InlineKeyboardButton
                    {
                        CallbackData = "-1",
                        Text = Chat.Texts.NoRemind
                    }
                },
                new[]
                {
                    new InlineKeyboardButton
                    {
                        CallbackData = BotCommands.Start,
                        Text = Chat.Texts.CancelButton,
                    }
                }
            });

        user.SetRemindFrequency(int.Parse(await Chat.WaitInlineKeyboardInput()));
        await _userService.Update(user);

        await Chat.SendMarkdownMessageAsync(
            Markdown.Escaped("Настройки установлены!"));
        
        Chat.ChatIo.OnUpdate(new Update {Message = new Message {Text = "/start"}});
   //?     throw new ProcessInterruptedWithMenuCommand(argument, botCommandHandler);
        

    }
}
}