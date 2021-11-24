using System;
using System.Linq;
using System.Text;
using Chotiskazal.Bot.InterfaceLang;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Users;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ChatFlows {
public static class StatsRenderer {
    private const string Empty = "✖";
    private const string S0 = "➖";
    private const string S1 = "▫️";
    private const string S2 = "◽️";
    private const string S3 = "◻️";
    private const string S4 = "⬜️";
    private const string S5 = Emojis.GreenSquare;
    private const string S6 = Emojis.Fire;

    public static string GetStatsText(ExamSettings settings, ChatRoom chat) =>
        RenderStats(settings, chat) +
        $"```\r\n" +
        Render7WeeksCalendar(settings, chat.User.LastDaysStats.Select(d => new CalendarItem(d.Date, d.LearningDone, d.GameScoreChanging)).ToArray(), chat.Texts) +
        $"```\r\n" +
        $"\r\n" +
        $"*{RenderRecomendations(chat.User, chat.Texts)}*";
    private static string Render7WeeksCalendar(
        ExamSettings examSettings, CalendarItem[] items, IInterfaceTexts texts) {
        var today = DateTime.Today;
        var offsets = items.ToDictionary(
            i => (int)(today - i.Date.Date).TotalDays,
            k => k.ExamsCount / (double)examSettings.ExamsCountGoalForDay
        );
        //7 weeks. 42-49 days
        var minDay = today.AddDays(-49);
        var undoneInLastWeek = 0;
        if (minDay.DayOfWeek != DayOfWeek.Sunday)
            undoneInLastWeek = (7 - (int)minDay.DayOfWeek);


        var sb = new StringBuilder("----------------------\r\n");

        for (int day = 0; day < 7; day++) {
            sb.Append(texts.ShortDayNames[day] + " ");
            for (int week = 7; week > 0; week--) {
                var offset = 7 * week - undoneInLastWeek - day - 1;
                if (offset < 0)
                    sb.Append(Empty);
                else if (offsets.TryGetValue(offset, out var v)) {
                    var symbol
                        = v < 0.1 ? S1
                        : v < 0.2 ? S2
                        : v < 0.5 ? S3
                        : v < 1.0 ? S4
                        : v <= 2.0 ? S5
                        : S6;
                    sb.Append(symbol);
                }
                else
                    sb.Append(S0);
            }

            sb.Append("\r\n");
        }
        sb.Append("----------------------\r\n ");
        sb.Append($"{texts.less} {S1}{S2}{S3}{S4}{S5} {texts.more}\r\n");
        return sb.ToString();
    }

    private static string RenderStats(ExamSettings settings, ChatRoom chat) {
        var lastMonth = chat.User.GetLastMonth();
        var lastDay = chat.User.GetToday();
        var statsText = $"{chat.Texts.StatsYourStats}: \r\n```\r\n" +
                        $"  {chat.Texts.StatsWordsAdded}: {chat.User.WordsCount}\r\n" +
                        $"  {chat.Texts.StatsLearnedWell}: {chat.User.CountOf((int)WordLeaningGlobalSettings.LearnedWordMinScore, 10)}\r\n" +
                        $"  {chat.Texts.StatsScore}: {(int)chat.User.GamingScore}\r\n```\r\n" +
                        $"{chat.Texts.StatsThisMonth}:\r\n```" +
                        $"  {chat.Texts.StatsWordsAdded}: {lastMonth.WordsAdded}\r\n" +
                        $"  {chat.Texts.StatsLearnedWell}: {lastMonth.WordsLearnt}\r\n" +
                        $"  {chat.Texts.StatsExamsPassed}: {lastMonth.LearningDone}\r\n" +
                        $"  {chat.Texts.StatsScore}: {(int)lastMonth.GameScoreChanging}\r\n```\r\n" +
                        $"{chat.Texts.StatsThisDay}:\r\n```" +
                        $"  {chat.Texts.StatsWordsAdded}: {lastDay.WordsAdded}\r\n" +
                        $"  {chat.Texts.StatsLearnedWell}: {lastDay.WordsLearnt}\r\n" +
                        $"  {chat.Texts.StatsExamsPassed}: {lastDay.LearningDone}/{settings.ExamsCountGoalForDay}\r\n" +
                        $"  {chat.Texts.StatsScore}: {(int)lastDay.GameScoreChanging}\r\n```\r\n" +
                        $" {chat.Texts.StatsActivityForLast7Weeks}:\r\n";
        return statsText;
    }

    private static string RenderRecomendations(UserModel user, IInterfaceTexts texts) {
        if (user.Zen.Rate < -15)
            return texts.Zen1WeNeedMuchMoreNewWords;
        else if (user.Zen.Rate < -10)
            return texts.Zen2TranslateNewWords;
        else if (user.Zen.Rate < -5)
            return texts.Zen3TranslateNewWordsAndPassExams;
        else if (user.Zen.Rate < 5)
            return texts.Zen3EverythingIsGood;
        else if (user.Zen.Rate < 10)
            return texts.Zen4PassExamsAndTranslateNewWords;
        else if (user.Zen.Rate < 20)
            return texts.Zen5PassExams;
        else
            return texts.Zen6YouNeedToLearn;
    }
}
}