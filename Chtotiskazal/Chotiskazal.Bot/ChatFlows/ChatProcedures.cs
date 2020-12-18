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
        public const string d = "➖";//"▪️"; //"➖"//"✖";//"◾";//"✖";//"⬛";
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
           
           
            var sb = new StringBuilder( "----------------------\r\n");

            for (int day = 0; day < 7; day++)
            {
                sb.Append(Texts.Current.ShortDayNames[day] + " ");
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
            sb.Append("----------------------\r\n ");
            sb.Append($"{Texts.Current.less} " + r + o + y + g + best + $" {Texts.Current.more}\r\n");
            return sb.ToString();
        }

        public static async Task ShowStats(ChatIO chatIo, UserModel userModel)
        {

            string recomendation = "";
            if (userModel.Zen.Rate < -15)
                recomendation = Texts.Current.Zen1WeNeedMuchMoreNewWords;
            else if (userModel.Zen.Rate < -10)
                recomendation = Texts.Current.Zen2TranslateNewWords;
            else if (userModel.Zen.Rate < -5)
                recomendation = Texts.Current.Zen3TranslateNewWordsAndPassExams;
            else if (userModel.Zen.Rate < 5)
                recomendation = Texts.Current.Zen3EverythingIsGood;
            else if (userModel.Zen.Rate < 10)
                recomendation = Texts.Current.Zen4PassExamsAndTranslateNewWords;
            else if (userModel.Zen.Rate < 15)
                recomendation = Texts.Current.Zen5PassExams;
            else 
                recomendation = Texts.Current.Zen6YouNeedToLearn;

            var weekStart = DateTime.Today.AddDays(-(int) DateTime.Today.DayOfWeek);
            var lastWeek = userModel.LastDaysStats.Where(w => w.Date >= weekStart).ToArray();
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
                $" {Texts.Current.StatsActivityForLast7Weeks}:\r\n```\r\n{Render7WeeksCalendar(userModel.LastDaysStats.Select(d => new CalendarItem(d.Date, d.GameScoreChanging)).ToArray())}```\r\n" +
                $"\r\n" +
                $"*{recomendation}*";
            await chatIo.SendMarkdownMessageAsync(msg.EscapeForMarkdown(),
                new[]{new[]{
                        InlineButtons.Exam, InlineButtons.MainMenu}, 
                    new[]{ InlineButtons.Translation}});
        }
    }
}