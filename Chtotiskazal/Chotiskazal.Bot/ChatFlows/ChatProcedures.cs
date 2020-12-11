using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SayWhat.MongoDAL.Users;

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
        
        public const string r = "🟥";
        public const string y = "🟨";
        public const string g = "🟩";
        public const string o = "🟧";
        public const string w = "⬜";
        public const string d = "✖";//"◾";//"✖";//"⬛";
        public const string n = "◾";
        public const string s = " ";
        public const string td = "👉";


        public static string Render7WeeksCalendar(CalendarItem[] items)
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
                        var symbol = v <= 0.25 ? r
                            : v <= 0.5 ? o
                            : v <= 0.75 ? y
                            : g;
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

            var msg = "Your stats: \r\n" +
                      $"Words: {userModel.WordsCount}\r\n" +
                      $"Translations: {userModel.PairsCount}\r\n" +
                      $"Examples: {userModel.ExamplesCount}" +
                      $"\r\n" +
                      $"```" +
                      Render7WeeksCalendar(userModel.LastDaysStats.Select(d => new CalendarItem(d.Date,
                          d.WordsLearnt * 100
                          + d.QuestionsPassed * 10
                          + d.QuestionsFailed * (-20)
                          + d.LearningDone * 100
                          + d.WordsAdded * 100)).ToArray()) +
                      
                      $"```\r\n" +
                      $"Words learned:\r\n" +
                      $"Total: {userModel.WordsLearned}\r\n" +
                      $"total bycl: {userModel.CountOf(4,10)}\r\n" +
                      $"Last month: {userModel.GetLastMonth().WordsLearnt}\r\n" +
                      $"Last week : {userModel.GetLastWeek().Sum(s => s.WordsLearnt)}\r\n" +
                      $"Today     : {userModel.GetToday().WordsLearnt}\r\n" +
                      $"Today bycl: {userModel.GetToday().CummulativeStatsChanging.CountOf(4,10)}\r\n" +

                      $"Score changing:\r\n" +
                      $"Last month: {userModel.GetLastMonth().CummulativeStatsChanging.AbsoluteScoreChanging}\r\n" +
                      $"Last week : {userModel.GetLastWeek().Sum(s => s.CummulativeStatsChanging.AbsoluteScoreChanging)}\r\n" +
                      $"Today     : {userModel.GetToday().CummulativeStatsChanging.AbsoluteScoreChanging}\r\n" +
                      $"\r\n" +
                      $"Outdated:\r\n" +
                      $"Total: {userModel.OutdatedWordsCount}\r\n" +
                      $"Last month: {userModel.GetLastMonth().CummulativeStatsChanging.OutdatedChanging}\r\n" +
                      $"Last week : {userModel.GetLastWeek().Sum(s => s.CummulativeStatsChanging.OutdatedChanging)}\r\n" +
                      $"Today     : {userModel.GetToday().CummulativeStatsChanging.OutdatedChanging}\r\n" +
                      $"a0: {userModel.CountOf(0, 2)}\r\n" +
                      $"a1: {userModel.CountOf(2, 4)}\r\n" +
                      $"a2: {userModel.CountOf(4, 6)}\r\n" +
                      $"a3: {userModel.CountOf(6, 9)}\r\n" +
                      $"Zento: {(userModel.CountOf(0,1) *4 + userModel.CountOf(1,2) *3+userModel.CountOf(2,3) *2+ userModel.CountOf(3,4) *1)}";
            await chatIo.SendMarkdownMessageAsync(msg.Replace("-", "\\-"));
        }
    }
}