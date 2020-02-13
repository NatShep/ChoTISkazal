using System;
using System.Diagnostics;
using System.Linq;
using Chotiskazal.App.Exams;
using Chotiskazal.Logic.DAL;
using Chotiskazal.Logic.Services;
using Dic.Logic;

namespace Chotiskazal.App.Modes
{
    public class ExamMode : IConsoleMode
    {
        public string Name => "Examination";

        public void Enter(NewWordsService service)
        {
            service.UpdateAgingAndRandomize(50);

            Console.WriteLine("Examination");
            var words = service.GetPairsForTest(9, 3);
            Console.Clear();
            Console.WriteLine("Examination: ");
            if (words.Average(w => w.PassedScore) <= 4)
            {
                foreach (var pairModel in words.Randomize())
                {
                    Console.WriteLine($"{pairModel.OriginWord}\t\t:{pairModel.Translation}");
                }
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to start an examination");
            Console.ReadKey();
            Console.Clear();
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
                        Stopwatch sw = Stopwatch.StartNew();
                        var questionMetric = new QuestionMetric
                        {
                            AggregateScoreBefore = pairModel.AggregateScore,
                            WordId =  pairModel.Id,
                            Created = DateTime.Now,
                            ExamsPassed = pairModel.Examed,
                            PassedScoreBefore = pairModel.PassedScore,
                            PhrasesCount = pairModel.Phrases?.Count ?? 0,
                            PreviousExam = pairModel.LastExam,
                            Type = exam.Name,
                            WordAdded = pairModel.Created
                        };
                        
                        var result = exam.Pass(service, pairModel, words);
                        sw.Stop();
                        questionMetric.ElaspedMs = (int)sw.ElapsedMilliseconds;
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
                                questionMetric.Result = 1;
                                service.SaveQuestionMetrics(questionMetric);
                                examsCount++;
                                examsPassed++;
                                break;
                            case ExamResult.Failed:
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("\r\n[failed]");
                                Console.ResetColor();
                                Console.WriteLine();
                                Console.WriteLine();
                                questionMetric.Result = 0;
                                service.SaveQuestionMetrics(questionMetric);
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


                    service.RegistrateExam(started, examsCount, examsPassed);


                }

            }

            Console.WriteLine();
            Console.WriteLine($"Test done:  {examsPassed}/{examsCount}");
            foreach (var pairModel in words)
            {
                Console.WriteLine(pairModel.OriginWord + " - " + pairModel.Translation + "  (" + pairModel.PassedScore +
                                  ")");
            }

            Console.WriteLine();
        }
    }
}
