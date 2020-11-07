using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Chotiskazal.ApI.Exams;
using Chotiskazal.Api.Services;
using Chotiskazal.ConsoleTesting.Services;
using Chotiskazal.DAL.Services;
using Chotiskazal.LogicR;

namespace Chotiskazal.Api.ConsoleModes
{
    public class ExamMode : IConsoleMode
    {
        public string Name => "Examination";

        private ExamService _examService;

        public ExamMode(ExamService examService) => _examService = examService;
        
        public void Enter(int userId)
        {
            //TODO Как добавляет слова( просто рандомно иногда добавляет фразы независимо от того, какой рейтинг выученности слов)
            _examService.RandomizationAndJobs(userId);
            
            var learningWords = _examService.GetWordsForLearning(userId, 9, 3);

            Console.Clear();
            Console.WriteLine("Examination: ");

            // print words and translates if Average PassedScore <=4
            if (learningWords.Average(w => w.PassedScore) <= 4)
            {
                foreach (var userWord in learningWords.Randomize())
                    Console.WriteLine($"{userWord.EnWord}\t\t:{userWord.UserTranslations}");
            }
            //Get exam list and test words
            var examsList =  _examService.PreparingExamsList(learningWords);
            var testWords = _examService.GetTestWords(userId, examsList);
            examsList.AddRange(testWords);

            //Begin test
            Console.WriteLine();
            Console.WriteLine("Press any key to start an examination");
            Console.ReadKey();
            Console.Clear();
            
            int examsCount = 0;
            int examsPassed = 0;
            DateTime started = DateTime.Now;
            int i = 0;
            ExamResult? lastExamResult = null;

            foreach (var userWord in examsList)
            {
                Console.WriteLine();
                var exam = ExamSelector.GetNextExamFor(i < 9, userWord);
                i++;
                bool retryFlag = false;
                do
                {
                    retryFlag = false;
                    Stopwatch sw = Stopwatch.StartNew();
                    var questionMetric = _examService.CreateQuestionMetric(userWord, exam);

                    var learnList = learningWords;

                    if (!learningWords.Contains(userWord))
                        learnList = learningWords.Append(userWord).ToArray();

                    if (exam.NeedClearScreen)
                    {
                        if (lastExamResult == ExamResult.Failed)
                        {
                            Console.WriteLine();
                            Console.WriteLine();
                            Console.WriteLine("Press any key to clear the screen...");
                            Thread.Sleep(100);
                            Console.ReadKey();
                        }

                        if (lastExamResult != ExamResult.Impossible)
                        {
                            Console.Clear();
                            if (lastExamResult == ExamResult.Passed)
                                WritePassed();
                        }
                    }

                    var result = exam.Pass(_examService, userWord, learnList);
                    sw.Stop();
                    questionMetric.ElaspedMs = (int) sw.ElapsedMilliseconds;
                    switch (result)
                    {
                        case ExamResult.Impossible:
                            exam = ExamSelector.GetNextExamFor(i == 0, userWord);
                            retryFlag = true;
                            break;
                        case ExamResult.Passed:
                            WritePassed();
                            _examService.SaveQuestionMetrics(questionMetric);
                            examsCount++;
                            examsPassed++;
                            break;
                        case ExamResult.Failed:
                            WriteFailed();
                            questionMetric.Result = 0;
                            _examService.SaveQuestionMetrics(questionMetric);
                            examsCount++;
                            break;
                        case ExamResult.Retry:
                            retryFlag = true;
                            Console.WriteLine();
                            Console.WriteLine();
                            break;
                        case ExamResult.Exit: return;
                    }
                    lastExamResult = result;

                } while (retryFlag);
                _examService.RegistrateExam(userId, started, examsCount, examsPassed);
            }

            Console.WriteLine();
            Console.WriteLine($"Test done:  {examsPassed}/{examsCount}");
            foreach (var pairModel in learningWords.Concat(testWords))
            {
                Console.WriteLine(pairModel.EnWord + " - " + pairModel.UserTranslations + "  (" + pairModel.PassedScore +
                                  ")");
            }
            
            Console.WriteLine();
        }

        private static void WriteFailed()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\r\n[failed]");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine();
        }

        private static void WritePassed()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\r\n[PASSED]");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine();
        }
    }
}
