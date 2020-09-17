using Chotiskazal.DAL;
using System;
using System.Linq;

namespace ConsoleTesting.Modes
{
    class GraphsStatsMode: IConsoleMode
    {
        public string Name => "Show stats";
        public void Enter(User user)
        {
           
        }

        private static void RenderKnowledgeHistogram()
        {
           

        }

        private static int GetLearningRate()
        {
            return 0;
        }

        private static void RenderAddingTimeLine()
        {
         

        }
        private static void RenderExamsTimeLine(Exam[] exams)
        {
            var wordTimeline = new int[21];
            int maxCount = 0;
            Console.ForegroundColor = ConsoleColor.DarkRed;

            foreach (var pairModel in exams)
            {
                var score = (int)(DateTime.Now.Date - pairModel.Started.Date).TotalDays + 1;
                if (score > wordTimeline.Length || score < 0)
                    continue;
                wordTimeline[^score]++;
                maxCount = Math.Max(wordTimeline[^score], maxCount);
            }
           
            int height = 15;
            for (int line = 0; line < height; line++)
            {
                Console.Write(" |");

                for (int row = 0; row < wordTimeline.Length; row++)
                {
                    var rowHeight = Math.Ceiling(((height * wordTimeline[row]) / (double)maxCount));
                    if (rowHeight >= height - line)
                        Console.Write("|_| ");
                    else
                        Console.Write("    ");
                }

                Console.Write("|\r\n");
            }

            Console.Write(" |");
            for (int row = 0; row < wordTimeline.Length; row++)
            {
                Console.Write("____");
            }

            Console.Write("|\r\n");
            Console.Write("     Exams History (v: exams amount, h: days)\r\n");
            Console.Write("  ");

            Console.ResetColor();

        }
    }
}
