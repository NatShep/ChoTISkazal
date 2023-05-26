using System;
using System.Threading;
using System.Threading.Tasks;
using Chotiskazal.Bot.Interface;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Users;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Timer = System.Timers.Timer;

namespace Chotiskazal.Bot.Jobs {
public static class RemindSenderJob {

    private static DateTime _launchTime;
    private static Timer _timer;

    public static async Task  Launch(TelegramBotClient botClient, UserService userService) {
        _launchTime = DateTime.Today.AddHours(15).AddMinutes(32);
    
        var now = DateTime.Now;
        var delay = now <= _launchTime
            ? (_launchTime - now).TotalMilliseconds
            : TimeSpan.FromDays(1).TotalMilliseconds - (now - _launchTime).TotalMilliseconds;
        await Task.Delay((int)delay);
        await RemindForAllUsers(botClient, userService);

        _timer = new Timer(TimeSpan.FromDays(1).TotalMilliseconds);

        _timer.Elapsed += async (_, _) => await RemindForAllUsers(botClient, userService);
        _timer.Enabled = true;
    }

    private static async Task RemindForAllUsers(TelegramBotClient botClient, UserService userService) {
        var users = userService.GetAllUsers();
        
        foreach (var user in users) {
            if (user.TelegramId.ToString() == "326823645") {
                var remindFrequency = user.RemindFrequency ?? 1;
                if (remindFrequency < 0)
                    continue;
                if (user.LastReminder is null || user.LastReminder?.AddDays(remindFrequency) >= DateTime.Now) {
                    await Remind(botClient, user);
                }
            }
        }
    }

    private static async Task Remind(TelegramBotClient botClient, UserModel user) {
        if (user.LastActivity.AddDays(1) <= DateTime.Today) {
            return;
        }
        
        var sb = Markdown.Escaped("Hey! Do you remember about me! Let's repeat words!\r\n");
        var chat = await botClient.GetChatAsync(user.TelegramId, CancellationToken.None);
        var chatRoom = Program.GetOrCreate(chat);
        
        var examBtn = InlineButtons.Exam($"Учи слова {Emojis.Learning}");
        var cnlBtn = InlineButtons.MainMenu("Нееее...");
        var settingsBtn = InlineButtons.Settings("Настройки");

        await botClient.SendTextMessageAsync(
            user.TelegramId, sb.GetMarkdownString(),
            replyMarkup: new InlineKeyboardMarkup(
                new[]
                {
                    new[]
                    {
                        examBtn, cnlBtn
                    },
                    new[]
                    {
                        settingsBtn
                    }
                }),

            parseMode: ParseMode.MarkdownV2);
    }
}
}