using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SayWhat.MongoDAL.Users;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ChatFlows
{
    public class CalendarItem
    {
        public CalendarItem(DateTime date, double score)
        {
            Date = date;
            Score = score;
        }

        public DateTime Date { get; }
        public double Score { get; }
    }
    public static class ChatProcedures
    {
        
        public const string b = "🟦";
        public const string v = "🟪";
        public const string w = "⬜";
        public const string s = " ";
        public const string td = "👉";

        public const string r = "▫️";//"🟥";
        public const string o = "◽️";//"🟧";
        public const string y = "◻️";//"🟨";
        public const string g = "⬜️";//"🟩";
        public const string best = "🟩";//"🟩";
        public const string d = "➖";//"▪️";//"✖";//"◾";//"✖";//"⬛";
        public const string n = "✖";//"◾";


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
           
            string[] dayNames = {
                "mon",
                "tue",
                "wed",
                "thu",
                "fri",
                "sat",
                "sun"
            };
            
            string[] monthNames = {
                "January",
                "February",
                "March",
                "April",
                "May",
                "June",
                "July",
                "August",
                "September",
                "October",
                "November",
                "December"
            };
            var sb = new StringBuilder( "-------------------------\r\n");

            //how many weeks ago month starts
            var weeksLastMonthStarts =  today.Day / 7;
            // char position
            var monthName = monthNames[today.Month-1];
            var monthTitleOffset = 4 + (7 - weeksLastMonthStarts) * 2.5 - monthName.Length-2;
            sb.Append(new string(' ', (int)monthTitleOffset) + monthName + "\r\n");
            for (int day = 0; day < 7; day++)
            {
                sb.Append(dayNames[day] + " ");
                for (int week = 7; week > 0; week--)
                {
                    var offset = 7 * week - undoneInLastWeek - day - 1;
                    if (offset < 0)
                        sb.Append(n);
                    else if (offsets.TryGetValue(offset, out var v))
                    {
                        var symbol = v <= 0.1 ? r
                            : v <= 0.2 ? o
                            : v <= 0.5 ? y
                            : v<= 0.9? g
                            : best;
                        sb.Append(symbol);
                    }
                    else
                        sb.Append(d);
                }

                sb.Append("\r\n");
            }
            sb.Append("-------------------------\r\n ");
            return sb.ToString();
        }

        public static async Task ShowStats(ChatIO chatIo, UserModel userModel)
        {

            string recomendation = "";
            if (userModel.Zen.Rate < -15)
                recomendation += $"We need more words! You have to add at least {userModel.Zen.CountOfWordsNeedToBeAdded} more words.";
            else if (userModel.Zen.Rate < -10)
                recomendation += $"Add {userModel.Zen.CountOfWordsNeedToBeAdded} words.";
            else if (userModel.Zen.Rate < -5)
                recomendation += $"Add {userModel.Zen.CountOfWordsNeedToBeAdded} words and pass {userModel.Zen.CountOfLearningNeedToBeDone} exams.";
            else if (userModel.Zen.Rate < 5)
                recomendation += $"Everything is perfect! " +
                                 $"\r\nAdd {userModel.Zen.CountOfWordsNeedToBeAdded} words and pass {userModel.Zen.CountOfLearningNeedToBeDone} exams.";
            else if (userModel.Zen.Rate < 10)
                recomendation += $"Pass {userModel.Zen.CountOfLearningNeedToBeDone} exams and add {userModel.Zen.CountOfWordsNeedToBeAdded} words.";
            else if (userModel.Zen.Rate < 15)
                recomendation += $"Pass {userModel.Zen.CountOfLearningNeedToBeDone} exams";
            else 
                recomendation += $"Learning learning learning! You have to pass at least {userModel.Zen.CountOfLearningNeedToBeDone} more exams";

            var weekStart = DateTime.Today.AddDays(-(int) DateTime.Today.DayOfWeek);
            var lastWeek = userModel.LastDaysStats.Where(w => w.Date >= weekStart).ToArray();
            var lastMonth = userModel.GetLastMonth();
            var lastDay = userModel.GetToday();

            var msg = "Your stats: \r\n```\r\n" +
                $"  Words added: {userModel.WordsCount}\r\n" +
                $"  Learned well: {userModel.CountOf((int) WordLeaningGlobalSettings.LearnedWordMinScore, 10)}\r\n" +
                $"  Score: {userModel.GamingScore}\r\n```\r\n" +
                $"This month:\r\n```" +
                $"  New words: {lastMonth.WordsAdded}\r\n" +
                $"  Learned well: {lastMonth.WordsLearnt}\r\n" +
                $"  Exams passed: {lastMonth.LearningDone}\r\n" +
                $"  Score: {lastMonth.GameScoreChanging}\r\n```\r\n" +
                $"This week:\r\n```" +
                $"  New words: {lastWeek.Sum(l=>l.WordsAdded)}\r\n" +
                $"  Learned well: {lastWeek.Sum(l=>l.WordsLearnt)}\r\n" +
                $"  Exams passed: {lastWeek.Sum(l=>l.LearningDone)}\r\n" +
                $"  Score: {lastWeek.Sum(l=>l.GameScoreChanging)}\r\n```\r\n" +
                $"This day:\r\n```" +
                $"  New words: {lastDay.WordsAdded}\r\n" +
                $"  Learned well: {lastDay.WordsLearnt}\r\n" +
                $"  Exams passed: {lastDay.LearningDone}\r\n" +
                $"  Score: {lastDay.GameScoreChanging}\r\n```\r\n" +
                $" Last activity:\r\n" +
                $"```" +
                Render7WeeksCalendar(userModel.LastDaysStats
                    .Select(d => new CalendarItem(d.Date, d.GameScoreChanging)).ToArray()) +
                "```\r\n" +
                "Recomendation: \r\n"+recomendation;
            await chatIo.SendMarkdownMessageAsync(msg.Replace("-", "\\-").Replace(".","\\.").Replace("!","\\!"));
        }
    }
}