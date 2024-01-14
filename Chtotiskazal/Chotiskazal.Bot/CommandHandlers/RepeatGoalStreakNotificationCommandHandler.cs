using System;
using System.Threading.Tasks;
using SayWhat.Bll.Services;

namespace Chotiskazal.Bot.CommandHandlers;

public class RepeatGoalStreakNotificationCommandHandler : IBotCommandHandler
{
    private readonly UserService _userService;

    public RepeatGoalStreakNotificationCommandHandler(UserService userService)
    {
        _userService = userService;
    }

    public bool Acceptable(string text) => TryParseCommand(text) != null;

    public string ParseArgument(string text) => TryParseCommand(text)?.ToString();

    public async Task Execute(string argument, ChatRoom chat)
    {
        if (!int.TryParse(argument, out var minute))
            throw new InvalidOperationException($"Cannot parse minutes: {argument}");
        chat.User.NotificationState.ScheduledGoalStreakNotification = DateTime.Now.AddMinutes(minute);
        await _userService.Update(chat.User);
        await chat.SendMessageAsync(chat.Texts.MotivationSnoozeScheduled.GetMarkdownString());
    }

    private int? TryParseCommand(string text)
    {
        if (!text.StartsWith(BotCommands.SnoozeMotivationHeader))
            return null;
        var tail = text.Substring(BotCommands.SnoozeMotivationHeader.Length);
        if (!int.TryParse(tail, out var minuteCount))
            return null;
        return minuteCount;
    }
}