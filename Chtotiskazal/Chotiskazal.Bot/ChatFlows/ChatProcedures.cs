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

        private static string Render7WeeksCalendar(CalendarItem[] items)
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
                sb.Append(Texts.Current.ShortDayNames[day] + " ");
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
            sb.Append($"{Texts.Current.less} " + S1 + S2 + S3 + S4 + S5 + $" {Texts.Current.more}\r\n");
            return sb.ToString();
        }

        public static async Task ShowStats(ChatIO chatIo, UserModel userModel)
        {
            var lastMonth = userModel.GetLastMonth();
            var lastDay = userModel.GetToday();

            var msg =
                $"{Texts.Current.StatsYourStats}: \r\n```\r\n" +
                $"  {Texts.Current.StatsWordsAdded}: {userModel.WordsCount}\r\n" +
                $"  {Texts.Current.StatsLearnedWell}: {userModel.CountOf((int) WordLeaningGlobalSettings.LearnedWordMinScore, 10)}\r\n" +
                $"  {Texts.Current.StatsScore}: {(int)userModel.GamingScore}\r\n```\r\n" +
                $"{Texts.Current.StatsThisMonth}:\r\n```" +
                $"  {Texts.Current.StatsWordsAdded}: {lastMonth.WordsAdded}\r\n" +
                $"  {Texts.Current.StatsLearnedWell}: {lastMonth.WordsLearnt}\r\n" +
                $"  {Texts.Current.StatsExamsPassed}: {lastMonth.LearningDone}\r\n" +
                $"  {Texts.Current.StatsScore}: {(int)lastMonth.GameScoreChanging}\r\n```\r\n" +
                $"{Texts.Current.StatsThisDay}:\r\n```" +
                $"  {Texts.Current.StatsWordsAdded}: {lastDay.WordsAdded}\r\n" +
                $"  {Texts.Current.StatsLearnedWell}: {lastDay.WordsLearnt}\r\n" +
                $"  {Texts.Current.StatsExamsPassed}: {lastDay.LearningDone}\r\n" +
                $"  {Texts.Current.StatsScore}: {(int)lastDay.GameScoreChanging}\r\n```\r\n" +
                $" {Texts.Current.StatsActivityForLast7Weeks}:\r\n" +
                $"```\r\n" +
                $"{Render7WeeksCalendar(userModel.LastDaysStats.Select(d => new CalendarItem(d.Date, d.GameScoreChanging)).ToArray())}" +
                $"```\r\n" +
                $"\r\n" +
                $"*{GetRecomendationFor(userModel)}*";
            await chatIo.SendMarkdownMessageAsync(msg.EscapeForMarkdown(),
                new[]{new[]{
                        InlineButtons.Exam, InlineButtons.MainMenu}, 
                    new[]{ InlineButtons.Translation}});
        }

        private static string GetRecomendationFor(UserModel userModel)
        {
            if (userModel.Zen.Rate < -15)
                return Texts.Current.Zen1WeNeedMuchMoreNewWords;
            else if (userModel.Zen.Rate < -10)
                return Texts.Current.Zen2TranslateNewWords;
            else if (userModel.Zen.Rate < -5)
                return Texts.Current.Zen3TranslateNewWordsAndPassExams;
            else if (userModel.Zen.Rate < 5)
                return Texts.Current.Zen3EverythingIsGood;
            else if (userModel.Zen.Rate < 10)
                return Texts.Current.Zen4PassExamsAndTranslateNewWords;
            else if (userModel.Zen.Rate < 15)
                return Texts.Current.Zen5PassExams;
            else
                return Texts.Current.Zen6YouNeedToLearn;
        }
    }
}