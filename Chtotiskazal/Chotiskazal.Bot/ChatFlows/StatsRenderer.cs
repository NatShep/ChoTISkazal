using System;
using System.Linq;
using System.Text;
using Chotiskazal.Bot.Texts;
using SayWhat.Bll.Services;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ChatFlows;

public static class StatsRenderer {
    private const string Empty = "✖";
    private const string S0 = "➖";
    private const string S1 = "▫️";
    private const string S2 = "◽️";
    private const string S3 = "◻️";
    private const string S4 = "⬜️";
    private const string S5 = Emojis.GreenSquare;
    private const string S6 = Emojis.Fire;

    public static Markdown GetStatsTextMarkdown(ExamSettings settings, ChatRoom chat) =>
        RenderStatsMarkdown(settings, chat) +
        Render7WeeksCalendarMarkdown(settings, chat.User.LastDaysStats
                .Select(d => new CalendarItem(d.Date, d.LearningDone, d.GameScoreChanging))
                .ToArray(), chat.Texts)
            .ToQuotationMono()
            .NewLine(); //+
    //RenderRecomendationsMarkdown(chat.User, chat.Texts).ToSemiBold();

    private static Markdown Render7WeeksCalendarMarkdown(
        
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


        var sbWithMarkwownFormatted = new StringBuilder("----------------------\r\n");

        for (int day = 0; day < 7; day++) {
            sbWithMarkwownFormatted.Append(Markdown.Escaped(texts.ShortDayNames[day] + " ").GetMarkdownString());
            for (int week = 7; week > 0; week--) {
                var offset = 7 * week - undoneInLastWeek - day - 1;
                if (offset < 0)
                    sbWithMarkwownFormatted.Append(Empty);
                else if (offsets.TryGetValue(offset, out var v)) {
                    var symbol
                        = v < 0.1 ? S1
                        : v < 0.2 ? S2
                        : v < 0.5 ? S3
                        : v < 1.0 ? S4
                        : v <= 2.0 ? S5
                        : S6;
                    sbWithMarkwownFormatted.Append(symbol);
                }
                else
                    sbWithMarkwownFormatted.Append(S0);
            }

            sbWithMarkwownFormatted.Append("\r\n");
        }
        sbWithMarkwownFormatted.Append("----------------------\r\n ");
        sbWithMarkwownFormatted.Append($"{Markdown.Escaped(texts.less).GetMarkdownString()} {S1}{S2}{S3}{S4}{S5} {Markdown.Escaped(texts.more).GetMarkdownString()}\r\n");
        return Markdown.Bypassed(sbWithMarkwownFormatted.ToString());
    }

    private static Markdown RenderStatsMarkdown(ExamSettings settings, ChatRoom chat) {
        var lastMonth = chat.User.GetLastMonth();
        var lastDay = chat.User.GetToday();

        var statsTextMarkdown = Markdown.Escaped(chat.Texts.StatsYourStats + ":\r\n") +
                                Markdown.Escaped($"  {chat.Texts.StatsWordsAdded}: {chat.User.WordsCount}\r\n" +
                                                 $"  {chat.Texts.StatsLearnedWell}: {chat.User.CountOf((int) WordLeaningGlobalSettings.WellDoneWordMinScore/2, 10)}\r\n" +
                                                 $"  {chat.Texts.StatsScore}: {(int) chat.User.GamingScore}\r\n")
                                    .ToQuotationMono() +
                                Markdown.Escaped($"{chat.Texts.StatsThisMonth}:\r\n") +
                                Markdown.Escaped($"  {chat.Texts.StatsWordsAdded}: {lastMonth.WordsAdded}\r\n" +
                                                 $"  {chat.Texts.StatsLearnedWell}: {lastMonth.WordsLearnt}\r\n" +
                                                 $"  {chat.Texts.StatsExamsPassed}: {lastMonth.LearningDone}\r\n" +
                                                 $"  {chat.Texts.StatsScore}: {(int) lastMonth.GameScoreChanging}\r\n")
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

    /*
     private static Markdown RenderRecomendationsMarkdown(UserModel user, IInterfaceTexts texts) {

        if (user.Zen.Rate < -15)
            return Markdown.Escaped(texts.Zen1WeNeedMuchMoreNewWords);
        else if (user.Zen.Rate < -10)
            return Markdown.Escaped(texts.Zen2TranslateNewWords);
        else if (user.Zen.Rate < -5)
            return Markdown.Escaped(texts.Zen3TranslateNewWordsAndPassExams);
        else if (user.Zen.Rate < 5)
            return Markdown.Escaped(texts.Zen3EverythingIsGood);
        else if (user.Zen.Rate < 10)
            return Markdown.Escaped(texts.Zen4PassExamsAndTranslateNewWords);
        else if (user.Zen.Rate < 20)
            return Markdown.Escaped(texts.Zen5PassExams);
        else
            return Markdown.Escaped(texts.Zen6YouNeedToLearn);
    }
    */
}