using System;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.ChatFlows;
using Chotiskazal.Bot.Texts;
using SayWhat.Bll.Services;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL.Users;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.Jobs;

public class NotificationJob
{
    private readonly TelegramBotClient _botClient;
    private readonly UserService _userService;
    private readonly int _examsCountGoalForDay;
    private readonly ILogger _logger;

    public NotificationJob(
        TelegramBotClient botClient,
        UserService userService,
        int examsCountGoalForDay,
        ILogger logger)
    {
        _botClient = botClient;
        _userService = userService;
        _examsCountGoalForDay = examsCountGoalForDay;
        _logger = logger;
    }

    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(4);

    public void Launch() => Task.Run(async () =>
    {
        while (true)
        {
            await Task.Delay(_checkInterval);
            if (DateTime.UtcNow.Hour < 19 || DateTime.UtcNow.Hour > 21)
                continue;
            var users = _userService.GetAllUsers();
            foreach (var user in users)
            {
                // проверяем отложенную нотификацию по целям
                if (await TryHandleSnoozedGoalStreakUserNotification(user))
                    continue;
                // проверяем регулярную нотификацию по целям
                await HandleGoalStreakUserNotification(user);
            }
        }
    });

    /// <summary>
    /// Проверка на будильник цели
    /// </summary>
    private async Task<bool> TryHandleSnoozedGoalStreakUserNotification(UserModel user)
    {
        if (!user.NotificationState.NotificationEnabled)
            return false;
        if (user.NotificationState.ScheduledGoalStreakNotification == null ||
            user.NotificationState.LastNotification == null)
            return false;
        var todayCalendar = user.GetToday();

        // Пользователь сдал уже экзамены
        if (user.LastExam > user.NotificationState.LastNotification.Value
            || todayCalendar.LearningDone >= _examsCountGoalForDay)
        {
            //сбрасываем будильник
            user.NotificationState.ScheduledGoalStreakNotification = null;
            await _userService.Update(user);
            return false;
        }

        //Еще не настало время
        if (user.NotificationState.ScheduledGoalStreakNotification > DateTime.Now)
            return false;

        //---------------
        // Обрабатываем будильник
        user.NotificationState.ScheduledGoalStreakNotification = null;
        var texts = user.GetText();
        //Теперь пытаемся выяснить какую нотификацию надо перепослать. 
        if (await TrySendLearningAlmostDoneReminder(user, todayCalendar, texts))
            return true;
        if (await TrySentUserCanLooseGoalStreakTodayReminder(user, texts))
            return true;

        //Если не выяснии - то посылаем регулярную нотификацию
        await SendRegularLearnReminder(user, texts);
        return true;
    }

    private async Task HandleGoalStreakUserNotification(UserModel user)
    {
        if (user.QuestionAsked == 0)
            return;

        //пользователь пользовался ботом только что
        if (DateTime.Now - user.LastActivity < TimeSpan.FromMinutes(60))
            return;

        //пользователь не включил нотификации
        if (user.NotificationState.NotificationEnabled != true)
            return;

        var todayCalendar = user.GetToday();
        //пользователь выполнил цель на день
        if (todayCalendar.LearningDone >= _examsCountGoalForDay)
            return;

        var texts = user.GetText();

        if (await TrySendLearningAlmostDoneReminder(user, todayCalendar, texts))
            return;

        if (await TrySentUserCanLooseGoalStreakTodayReminder(user, texts))
            return;

        //Регулярная нотификация, с увеличивающимся периодом
        if (DoWeNeedToNotifyAboutLearning(user))
        {
            await SendRegularLearnReminder(user, texts);
            return;
        }
    }

    private Task SendRegularLearnReminder(UserModel user, IInterfaceTexts texts) =>
        SendGoalStreakMessageAndUpdateUser(user,
            texts.MotivationReminderLearn,
            new InlineKeyboardMarkup(new[]
            {
                InlineButtons.Learn(texts),
                InlineButtons.Settings(texts)
            }));

    private async Task<bool> TrySentUserCanLooseGoalStreakTodayReminder(UserModel user, IInterfaceTexts texts)
    {
        if (user.MaxGoalStreak <= 10)
            return false; //Эту опцию мы включаем только для крутых пользователей
        
        //Проверяем потеряет ли пользователь goalstreak если не выполнит сегодня цель на день
        var calendar = user.GetCalendar();
        var todayStats = calendar.FirstOrDefault(c => c.Date == DateTime.Today);
        var calendarWithTodayIsNotPassed = calendar
            .Where(c => c != todayStats)
            .Append(new CalendarItem(DateTime.Today, 0, 0))
            .ToArray();
        var goalStreakIfTodayIsNotPassed =
            StatsHelper.GetGoalsStreak(calendarWithTodayIsNotPassed, _examsCountGoalForDay);
        var calendarWithTodayIsPassed = calendar
            .Where(c => c != todayStats)
            .Append(new CalendarItem(DateTime.Today, _examsCountGoalForDay, 0))
            .ToArray();
        var goalStreakIfTodayIsPassed =
            StatsHelper.GetGoalsStreak(calendarWithTodayIsPassed, _examsCountGoalForDay);
        
        var canUserLooseGoalStreakToday =
            goalStreakIfTodayIsNotPassed.GoalStreakCount < goalStreakIfTodayIsPassed.GoalStreakCount;

        if (!canUserLooseGoalStreakToday)
            return false;

        await SendGoalStreakMessageAndUpdateUser(user,
            texts.MotivationYouCanLooseGoalStreakToday(goalStreakIfTodayIsNotPassed.GoalStreakCount),
            new InlineKeyboardMarkup(
                new[]
                {
                    InlineButtons.Learn(texts),
                    InlineButtons.SnoozeGoalStreak15(texts),
                    InlineButtons.SnoozeGoalStreak60(texts),
                    InlineButtons.Settings(texts)
                }
            ));
        return true;
    }

    private async Task<bool> TrySendLearningAlmostDoneReminder(UserModel user, DailyStats todayCalendar, IInterfaceTexts texts)
    {
        if (todayCalendar.LearningDone < _examsCountGoalForDay / 2) return false;

        //Пользователь выполнил более половины цели на день. Нужно его приободрить!
        await SendGoalStreakMessageAndUpdateUser(user,
            texts.MotivationYouAlmostFinishedGoalForTheDay,
            new InlineKeyboardMarkup(
                new[]
                {
                    InlineButtons.Learn(texts),
                    InlineButtons.SnoozeGoalStreak15(texts),
                    InlineButtons.SnoozeGoalStreak60(texts),
                    InlineButtons.Settings(texts)
                }
            ));
        return true;
    }

    private async Task SendGoalStreakMessageAndUpdateUser(
        UserModel user,
        Markdown message,
        InlineKeyboardMarkup keyboard = null)
    {
        user.NotificationState.OnGoalStreakMessage();
        try
        {
            await _botClient.SendTextMessageAsync(
                user.TelegramId,
                message.GetMarkdownString(),
                replyMarkup: keyboard,
                parseMode: ParseMode.MarkdownV2);
        }
        catch (Exception e)
        {
            user.NotificationState.LastNotificationError = e.GetType().Name + ":" + e.Message;
        }

        await _userService.Update(user);
    }

    private bool DoWeNeedToNotifyAboutLearning(UserModel user)
    {
        if (user.QuestionAsked == 0)
            return false;
        if (user.WordsCount < 10)
            return false;
        //Напоминаем пользователю о том что нужно пройти экзамены если есть хоть одно слово
        //Последовательность пауз в днях: 3, 6, 12, 24, 48, 96, 96, 96...
        var lastNotificationTime = user.NotificationState.LastNotification;

        if (lastNotificationTime == null || user.LastExam.Date > lastNotificationTime.Value)
        {
            //Мы никогда не оповещали пользователя, значит берем минимальное время оповещения
            return (DateTime.Today - user.LastExam.Date).TotalDays > 3;
        }

        //Столько времени прошло с момента последнего обучения до последней нотификации
        var lastNotificationPause = (lastNotificationTime.Value.Date - user.LastExam.Date).Days;
        //Высчитываем когда нужно провести следующую нотификацию
        var nextNotificationDate = lastNotificationPause switch
        {
            <= 3 => lastNotificationTime.Value.Date.AddDays(6),
            <= 3 + 6 => lastNotificationTime.Value.Date.AddDays(12),
            <= 3 + 6 + 12 => lastNotificationTime.Value.Date.AddDays(24),
            <= 3 + 6 + 12 + 24 => lastNotificationTime.Value.Date.AddDays(48),
            _ => lastNotificationTime.Value.Date.AddDays(96)
        };
        return nextNotificationDate < DateTime.Now;
    }
}