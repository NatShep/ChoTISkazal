using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chotiskazal.Bot.InterfaceLang;
using SayWhat.Bll;
using SayWhat.MongoDAL.Users;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ChatFlows
{
    public static class ChatProcedures
    {
        private const string Empty = "✖";
        private const string S0 = "➖";
        private const string S1 = "▫️";
        private const string S2 = "◽️";
        private const string S3 = "◻️";
        private const string S4 = "⬜️";
        private const string S5 = "🟩";

        private static string Render7WeeksCalendar(CalendarItem[] items, IInterfaceTexts texts)
        {
            var today = DateTime.Today;
            double minVal = double.MaxValue;
            double maxVal = 0;
            foreach (var item in items)
            {
                minVal = Math.Min(minVal, item.Score);
                maxVal = Math.Max(maxVal, item.Score);
            }
            var offsets = items.ToDictionary(
                i => (int) (today - i.Date.Date).TotalDays,
                k => (maxVal==minVal)? 1: (k.Score- minVal)/ (maxVal-minVal)
            );
            //7 weeks. 42-49 days
            var minDay = today.AddDays(-49);
            var undoneInLastWeek = 0;
            if (minDay.DayOfWeek != DayOfWeek.Sunday) 
                undoneInLastWeek = (7 - (int) minDay.DayOfWeek);
           
           
            var sb = new StringBuilder( "----------------------\r\n");

            for (int day = 0; day < 7; day++)
            {
                sb.Append(texts.ShortDayNames[day] + " ");
                for (int week = 7; week > 0; week--)
                {
                    var offset = 7 * week - undoneInLastWeek - day - 1;
                    if (offset < 0)
                        sb.Append(Empty);
                    else if (offsets.TryGetValue(offset, out var v))
                    {
                        var symbol = v <= 0.1 ? S1
                            : v <= 0.2 ? S2
                            : v <= 0.5 ? S3
                            : v<= 0.9? S4
                            : S5;
                        sb.Append(symbol);
                    }
                    else
                        sb.Append(S0);
                }

                sb.Append("\r\n");
            }
            sb.Append("----------------------\r\n ");
            sb.Append($"{texts.less} " + S1 + S2 + S3 + S4 + S5 + $" {texts.more}\r\n");
            return sb.ToString();
        }

        public static async Task ShowStats(ChatRoom chat)
        {
            var lastMonth = chat.User.GetLastMonth();
            var lastDay = chat.User.GetToday();

            var msg =
                $"{chat.Texts.StatsYourStats}: \r\n```\r\n" +
                $"  {chat.Texts.StatsWordsAdded}: {chat.User.WordsCount}\r\n" +
                $"  {chat.Texts.StatsLearnedWell}: {chat.User.CountOf((int) WordLeaningGlobalSettings.LearnedWordMinScore, 10)}\r\n" +
                $"  {chat.Texts.StatsScore}: {(int)chat.User.GamingScore}\r\n```\r\n" +
                $"{chat.Texts.StatsThisMonth}:\r\n```" +
                $"  {chat.Texts.StatsWordsAdded}: {lastMonth.WordsAdded}\r\n" +
                $"  {chat.Texts.StatsLearnedWell}: {lastMonth.WordsLearnt}\r\n" +
                $"  {chat.Texts.StatsExamsPassed}: {lastMonth.LearningDone}\r\n" +
                $"  {chat.Texts.StatsScore}: {(int)lastMonth.GameScoreChanging}\r\n```\r\n" +
                $"{chat.Texts.StatsThisDay}:\r\n```" +
                $"  {chat.Texts.StatsWordsAdded}: {lastDay.WordsAdded}\r\n" +
                $"  {chat.Texts.StatsLearnedWell}: {lastDay.WordsLearnt}\r\n" +
                $"  {chat.Texts.StatsExamsPassed}: {lastDay.LearningDone}\r\n" +
                $"  {chat.Texts.StatsScore}: {(int)lastDay.GameScoreChanging}\r\n```\r\n" +
                $" {chat.Texts.StatsActivityForLast7Weeks}:\r\n" +
                $"```\r\n" +
                $"{Render7WeeksCalendar(chat.User.LastDaysStats.Select(d => new CalendarItem(d.Date, d.GameScoreChanging)).ToArray(),chat.Texts)}" +
                $"```\r\n" +
                $"\r\n" +
                $"*{GetRecomendationFor(chat.User, chat.Texts)}*";
            await chat.SendMarkdownMessageAsync(msg.EscapeForMarkdown(),
                new[]{
                    new[]{
                         InlineButtons.MainMenu($"{Emojis.MainMenu} {chat.Texts.MainMenuButton}"),
                         InlineButtons.Exam($"{chat.Texts.LearnButton} {Emojis.Learning}"),}, 
                    new[]{ InlineButtons.Translation($"{chat.Texts.TranslateButton} {Emojis.Translate}")},
                    new[]{ InlineButtons.WellLearnedWords($"{chat.Texts.ShowWellKnownWords} {Emojis.ShowWellLearnedWords}")}
                });
        }

        private static string GetRecomendationFor(UserModel user, IInterfaceTexts texts)
        {
            if (user.Zen.Rate < -15)
                return texts.Zen1WeNeedMuchMoreNewWords;
            if (user.Zen.Rate < -10)
                return texts.Zen2TranslateNewWords;
            if (user.Zen.Rate < -5)
                return texts.Zen3TranslateNewWordsAndPassExams;
            if (user.Zen.Rate < 5)
                return texts.Zen3EverythingIsGood;
            if (user.Zen.Rate < 10)
                return texts.Zen4PassExamsAndTranslateNewWords;
            if (user.Zen.Rate < 15)
                return texts.Zen5PassExams;
            return texts.Zen6YouNeedToLearn;
        }
    }
}