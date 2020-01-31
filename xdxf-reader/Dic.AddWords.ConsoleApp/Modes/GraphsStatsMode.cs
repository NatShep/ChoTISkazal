using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dic.Logic.DAL;
using Dic.Logic.Services;

namespace Dic.AddWords.ConsoleApp.Modes
{
    class GraphsStatsMode: IConsoleMode
    {
        public string Name => "Show stats";
        public void Enter(NewWordsService service)
        {
            var allWords = service.GetAll();

            RenderKnowledgeHistogram(allWords);
            Console.WriteLine();
            Console.WriteLine();
            RenderAddingTimeLine(allWords);
            RenderExamsTimeLine(service.GetAllExams());

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            Console.WriteLine();    
            
            Console.WriteLine($"Context phrases count = {service.GetContextPhraseCount()}");
            Console.WriteLine($"Words count = {allWords.Length}");

            var groups = allWords
                .GroupBy(s => s.State)
                .OrderBy(s => (int)s.Key)
                .Select(s => new { state = s.Key, count = s.Count() });

            var doneCount = 0;
            foreach (var group in groups)
            {
                Console.WriteLine($"{group.state} {group.count}");
                if (group.state == LearningState.Done)
                    doneCount = group.count;
            }
            Console.WriteLine($"Done: {(doneCount * 100 / allWords.Length)}%");
            Console.WriteLine($"Unknown: {allWords.Length - doneCount} words");
        }

        private static void RenderKnowledgeHistogram(PairModel[] allWords)
        {
            var wordHystogramm = new int[15];

            int maxCount = 0;
            foreach (var pairModel in allWords)
            {
                var score = pairModel.PassedScore;
                if (score >= wordHystogramm.Length)
                    score = wordHystogramm.Length - 1;
                wordHystogramm[score]++;
                maxCount = Math.Max(wordHystogramm[score], maxCount);
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.Write("     Knowledge histogram (v: words amount, h: knowledge)\r\n");
            Console.Write("  ");
            Console.ForegroundColor = ConsoleColor.Yellow;

            for (int row = 0; row < wordHystogramm.Length; row++)
            {
                Console.Write("____");
            }

            Console.Write("\r\n");

            int height = 15;
            for (int line = 0; line < height; line++)
            {
                Console.Write(" |");

                for (int row = 0; row < wordHystogramm.Length; row++)
                {
                    var rowHeight = Math.Ceiling(((height * wordHystogramm[row]) / (double) maxCount));
                    if (rowHeight > height - line)
                        Console.Write("|_| ");
                    else
                        Console.Write("    ");
                }

                Console.Write("|\r\n");
            }

            Console.Write(" |");
            for (int row = 0; row < wordHystogramm.Length; row++)
            {
                Console.Write("____");
            }

            Console.Write("|\r\n");
            Console.ResetColor();

        }


        private static void RenderAddingTimeLine(PairModel[] allWords)
        {
            var wordTimeline = new int[21];

            int maxCount = 0;
            foreach (var pairModel in allWords)
            {
                var score = (int)(DateTime.Now- pairModel.Created).TotalDays +1;
                if(score>wordTimeline.Length || score<0)
                    continue;
                wordTimeline[^score]++;
                maxCount = Math.Max(wordTimeline[^score], maxCount);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("     Add History (v: words amount, h: days)\r\n");
            Console.Write("  ");

            for (int row = 0; row < wordTimeline.Length; row++)
            {
                Console.Write("____");
            }

            Console.Write("\r\n");

            int height = 15;
            for (int line = 0; line < height; line++)
            {
                Console.Write(" |");

                for (int row = 0; row < wordTimeline.Length; row++)
                {
                    var rowHeight = Math.Round(((height * wordTimeline[row]) / (double)maxCount));
                    if (rowHeight > height - line)
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
            Console.ResetColor();

        }
        private static void RenderExamsTimeLine(Exam[] exams)
        {
            var wordTimeline = new int[21];
            int maxCount = 0;
            Console.ForegroundColor = ConsoleColor.DarkRed;

            foreach (var pairModel in exams)
            {
                var score = (int)(DateTime.Now - pairModel.Started).TotalDays + 1;
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
                    var rowHeight = Math.Round(((height * wordTimeline[row]) / (double)maxCount));
                    if (rowHeight > height - line)
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
