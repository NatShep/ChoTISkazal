using System;
using System.Linq;
using System.Text;
using Chotiskazal.Bot.ChatFlows.FlowLearning;
using Chotiskazal.Bot.Texts;
using SayWhat.Bll.Services;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL;

namespace Chotiskazal.Bot.ChatFlows;

public static class StatsRenderer
{
    private const string Empty = "✖";
    private const string S0 = "➖";
    private const string S1 = "▫️";
    private const string S2 = "◽️";
    private const string S3 = "◻️";
    private const string S4 = "⬜️";
    private const string S5 = Emojis.GreenSquare;
    private const string S6 = Emojis.Fire;

    public static Markdown GetStatsTextMarkdown(ExamSettings settings, ChatRoom chat)
    {
        var msg = RenderStatsMarkdown(settings, chat);
        var calendat = chat.User.GetCalendar();
        msg += Render7WeeksCalendarMarkdown(settings, calendat, chat.Texts)
            .ToQuotationMono()
            .NewLine();
        var (goalStreakCount, hasGaps) = StatsHelper.GetGoalsStreak(calendat, settings.ExamsCountGoalForDay);
        msg += chat.Texts.GoalStreakStatsFooter(chat.User.MaxGoalStreak, goalStreakCount, hasGaps);
        return msg;
    }

    private static Markdown Render7WeeksCalendarMarkdown(
        ExamSettings examSettings, CalendarItem[] items, IInterfaceTexts texts)
    {
        var today = DateTime.Today;
        var offsets = items.GetOffsets(examSettings.ExamsCountGoalForDay);
        //7 weeks. 42-49 days
        var minDay = today.AddDays(-49);
        var undoneInLastWeek = 0;
        if (minDay.DayOfWeek != DayOfWeek.Sunday)
            undoneInLastWeek = (7 - (int)minDay.DayOfWeek);


        var sbWithMarkdownFormatted = new StringBuilder("----------------------\r\n");

        for (int day = 0; day < 7; day++)
        {
            sbWithMarkdownFormatted.Append(Markdown.Escaped(texts.ShortDayNames[day] + " ").GetMarkdownString());
            for (int week = 7; week > 0; week--)
            {
                var offset = 7 * week - undoneInLastWeek - day - 1;
                if (offset < 0)
                    sbWithMarkdownFormatted.Append(Empty);
                else if (offsets.TryGetValue(offset, out var v))
                {
                    var symbol = v switch
                    {
                        DayGoalResult.S1 => S1,
                        DayGoalResult.S2 => S2,
                        DayGoalResult.S3 => S3,
                        DayGoalResult.S4 => S4,
                        DayGoalResult.Goal => S5,
                        DayGoalResult.Overreaching => S6,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    // = v < 0.1 ? S1
                    // : v < 0.2 ? S2
                    // : v < 0.5 ? S3
                    // : v < 1.0 ? S4
                    // : v <= 2.0 ? S5
                    // : S6;
                    sbWithMarkdownFormatted.Append(symbol);
                }
                else
                    sbWithMarkdownFormatted.Append(S0);
            }

            sbWithMarkdownFormatted.Append("\r\n");
        }

        sbWithMarkdownFormatted.Append("----------------------\r\n ");
        sbWithMarkdownFormatted.Append(
            $"{Markdown.Escaped(texts.less).GetMarkdownString()} {S1}{S2}{S3}{S4}{S5} {Markdown.Escaped(texts.more).GetMarkdownString()}\r\n");

        return Markdown.Bypassed(sbWithMarkdownFormatted.ToString());
    }

    private static Markdown RenderStatsMarkdown(ExamSettings settings, ChatRoom chat)
    {
        var lastMonth = chat.User.GetLastMonth();
        var lastDay = chat.User.GetToday();

        var statsTextMarkdown = Markdown.Escaped(chat.Texts.StatsYourStats + ":\r\n") +
                                Markdown.Escaped($"  {chat.Texts.StatsWordsAdded}: {chat.User.WordsCount}\r\n" +
                                                 $"  {chat.Texts.StatsLearnedWell}: {chat.User.CountOf((int)(WordLeaningGlobalSettings.WellDoneWordMinScore / 2), 10)}\r\n" +
                                                 $"  {chat.Texts.StatsScore}: {(int)chat.User.GamingScore}\r\n")
                                    .ToQuotationMono() +
                                Markdown.Escaped($"{chat.Texts.StatsThisMonth}:\r\n") +
                                Markdown.Escaped($"  {chat.Texts.StatsWordsAdded}: {lastMonth.WordsAdded}\r\n" +
                                                 $"  {chat.Texts.StatsLearnedWell}: {lastMonth.WordsLearnt}\r\n" +
                                                 $"  {chat.Texts.StatsExamsPassed}: {lastMonth.LearningDone}\r\n" +
                                                 $"  {chat.Texts.StatsScore}: {(int)lastMonth.GameScoreChanging}\r\n")
                                    .ToQuotationMono() +
                                Markdown.Escaped($"{chat.Texts.StatsThisDay}:\r\n") +
                                Markdown.Escaped($"  {chat.Texts.StatsWordsAdded}: {lastDay.WordsAdded}\r\n" +
                                                 $"  {chat.Texts.StatsLearnedWell}: {lastDay.WordsLearnt}\r\n" +
                                                 $"  {chat.Texts.StatsExamsPassed}: {lastDay.LearningDone}/{settings.ExamsCountGoalForDay}\r\n" +
                                                 $"  {chat.Texts.StatsScore}: {(int)lastDay.GameScoreChanging}\r\n")
                                    .ToQuotationMono() +
                                Markdown.Escaped($" {chat.Texts.StatsActivityForLast7Weeks}:\r\n");

        return statsTextMarkdown;
    }
}