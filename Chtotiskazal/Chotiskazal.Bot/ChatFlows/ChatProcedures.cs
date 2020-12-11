using System.Linq;
using System.Threading.Tasks;
using SayWhat.MongoDAL.Users;

namespace Chotiskazal.Bot.ChatFlows
{
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
        public const string s = " ";
        public const string td = "👉";
        
        public static async Task ShowStats(ChatIO chatIo, UserModel userModel)
        {

            await chatIo.SendMarkdownMessageAsync("Your stats: \r\n" +
                                                  $"Words: {userModel.WordsCount}\r\n" +
                                                  $"Translations: {userModel.PairsCount}\r\n" +
                                                  $"Examples: {userModel.ExamplesCount}" +
                                                  $"\r\n" +
                                                  $"```" +
                                                  "\r\n -------------------------" +
                                                  "\r\n   december|  " +
                                                  $"\r\n mon {g + d + d + g + d + d + r + g}" +
                                                  $"\r\n tue {d + d + d + d + d + d + r + g}" +
                                                  $"\r\n wed {d + d + d + d + d + d + r + g}" +
                                                  $"\r\n thu {d + d + d + d + d + d + r + r}" +
                                                  $"\r\n fri {d + d + o + d + d + o + r + "-"}" +
                                                  $"\r\n sun {d + d + o + d + d + o + r + s}" +
                                                  $"\r\n sat {d + d + o + d + d + o + r + s}" +
                                                  "\r\n -------------------------" +
                                                  /*
                                                  $"\r\n[ 1 - 7] {d+d+d+g+y+o+r+r}" +
                                                  $"\r\n[ 1 - 7] {d+d+d+g+y+o+r+r}" +
                                                  $"\r\n[ 1 - 7] {d+d+d+g+y+o+r+r}" +
                                                  $"\r\n[ 1 - 7] {d+d+d+g+y+o+r+r}" +
                                                    */
                                                  /*$"\r\n[ 8 -14] {w+s+w+s+w+s+w+s+b+s+b+s+g}" +
                                                  $"\r\n[ 15-21] {r+s+g+s+g+s+g+s+g+s+g+s+b}" +
                                                  $"\r\n[ 22-28] {b+s+b+s+d+s+w+s+g+s+y+s+r}" +
                                                  $"\r\n[ 28- 3] {d+s+d+s+d+s+d+s+g+s+y+s+r}r\n" +*/

                                                  $"```\r\n" +
                                                  $"Words learned:\r\n" +
                                                  $"Total: {userModel.WordsLearned}\r\n" +
                                                  $"Last month: {userModel.GetLastMonth().WordsLearnt}\r\n" +
                                                  $"Last week : {userModel.GetLastWeek().Sum(s => s.WordsLearnt)}\r\n" +
                                                  $"Today     : {userModel.GetToday().WordsLearnt}\r\n" +
                                                  $"Score changing:\r\n" +
                                                  $"Last month: {userModel.GetLastMonth().CummulativeStatsChanging.AbsoluteScoreChanging}\r\n" +
                                                  $"Last week : {userModel.GetLastWeek().Sum(s => s.CummulativeStatsChanging.AbsoluteScoreChanging)}\r\n" +
                                                  $"Today     : {userModel.GetToday().CummulativeStatsChanging.AbsoluteScoreChanging}\r\n" +
                                                  $"\r\n" +
                                                  $"a0: {userModel.A0WordCount}\r\n" +
                                                  $"a1: {userModel.A1WordCount}\r\n" +
                                                  $"a2: {userModel.A2WordCount}\r\n" +
                                                  $"a3: {userModel.A3WordCount}\r\n" +
                                                  $"Zento: {userModel.LeftToA2}");


        }
    }
}