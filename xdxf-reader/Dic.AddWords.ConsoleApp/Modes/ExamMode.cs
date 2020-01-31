using System;
using System.Collections.Generic;
using System.Text;
using Dic.AddWords.ConsoleApp.Exams;
using Dic.Logic;
using Dic.Logic.Services;

namespace Dic.AddWords.ConsoleApp.Modes
{
    public class ExamMode: IConsoleMode
    {
        public string Name => "Examination";
        public void Enter(NewWordsService service)
        {
            Console.WriteLine("Examination");
            var words = service.GetPairsForTest(9, 3);
            Console.Clear();
            Console.WriteLine("Examination: ");
            foreach (var pairModel in words.Randomize())
            {
                Console.WriteLine($"{pairModel.OriginWord}\t\t:{pairModel.Translation}");
            }
            Console.WriteLine();
            Console.WriteLine("Press any key to start an examination");
            Console.ReadKey();
            Console.Clear();
            for (int examNum = 0; examNum < 2; examNum++)
            {
                int examsCount = 0;
                int examsPassed = 0;
                DateTime started = DateTime.Now;
                for (int i = 0; i < 3; i++)
                {
                    foreach (var pairModel in words.Randomize())
                    {
                        Console.WriteLine();
                        IExam exam;

                        exam = ExamSelector.GetNextExamFor(i == 0, pairModel);
                        bool retryFlag = false;
                        do
                        {
                            retryFlag = false;

                            var result = exam.Pass(service, pairModel, words);
                            switch (result)
                            {
                                case ExamResult.Impossible:
                                    exam = ExamSelector.GetNextExamFor(i == 0, pairModel);
                                    retryFlag = true;
                                    break;
                                case ExamResult.Passed:
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("\r\n[PASSED]");
                                    Console.ResetColor();
                                    Console.WriteLine();
                                    Console.WriteLine();
                                    examsCount++;
                                    examsPassed++;
                                    break;
                                case ExamResult.Failed:
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("\r\n[failed]");
                                    Console.ResetColor();
                                    Console.WriteLine();
                                    Console.WriteLine();
                                    examsCount++;
                                    break;
                                case ExamResult.Retry:
                                    retryFlag = true;
                                    Console.WriteLine();
                                    Console.WriteLine();
                                    break;
                                case ExamResult.Exit: return;
                            }
                        } while (retryFlag);

                    }
                }
                service.UpdateAgingAndRandomize();
                service.RegistrateExam(started, examsCount, examsPassed);

                Console.WriteLine();
                Console.WriteLine($"Test done:  {examsPassed}/{examsCount}");
                Console.WriteLine($"Repeat? [Y]es [N]o");
                var key = Console.ReadKey();
                if (key.Key != ConsoleKey.Y)
                    break;
            }

            foreach (var pairModel in words)
            {
                Console.WriteLine(pairModel.OriginWord + " - " + pairModel.Translation + "  (" + pairModel.PassedScore + ")");
            }
            Console.WriteLine();
        }
    }
}
