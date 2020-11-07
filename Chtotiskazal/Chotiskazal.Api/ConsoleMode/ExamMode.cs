using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Chotiskazal.Api.ConsoleModes;
using Chotiskazal.ApI.Exams;
using Chotiskazal.Api.Services;
using Chotiskazal.ConsoleTesting.Services;
using Chotiskazal.Dal;
using Chotiskazal.DAL;
using Chotiskazal.Dal.Services;
using Chotiskazal.LogicR;

namespace Chotiskazal.Api.ConsoleModes
{
    public class ExamMode : IConsoleMode
    {
        public string Name => "Examination";

       
        private AddWordService _addWordService;
        private ExamService _examService;

        public ExamMode(AddWordService addWordService, ExamService examService)
        {
            _addWordService = addWordService;
            _examService = examService;
        }
        public void Enter(int userId)
        {
            //Randomization and jobs
            //TODO просто рандомно иногда добавляет фразы независимо от того, какой рейтинг выученности слов
            if (RandomTools.Rnd.Next() % 30 == 0)
                _addWordService.AddMutualPhrasesToVocab(userId, 10);
            else
                _examService.UpdateAgingAndRandomize(50);
            
            //GetWordsForLearning
            var learningWords = _examService.GetWordsForLearning(userId, 9, 3);

            Console.Clear();
            Console.WriteLine("Examination: ");

            // print words and translates if Average PassedScore <=4
            if (learningWords.Average(w => w.PassedScore) <= 4)
            {
                foreach (var pairModel in learningWords.Randomize())
                    Console.WriteLine($"{pairModel.EnWord}\t\t:{pairModel.UserTranslations}");
            }
            //Get exam list and test words
            var examsList =  _examService.PreparingExamsList(learningWords);
            var testWords = _examService.GetTestWords(userId, examsList);
            
            examsList.AddRange(testWords);

            Console.WriteLine();
            Console.WriteLine("Press any key to start an examination");
            Console.ReadKey();
            Console.Clear();
            
            ///TODO make from this common method 
            int examsCount = 0;
            int examsPassed = 0;
            DateTime started = DateTime.Now;
            int i = 0;
            ExamResult? lastExamResult = null;

            foreach (var pairModel in examsList)
            {
                Console.WriteLine();
                var exam = ExamSelector.GetNextExamFor(i < 9, pairModel);
                
                i++;
                bool retryFlag = false;
                
                do
                {
                    retryFlag = false;
                    Stopwatch sw = Stopwatch.StartNew();
                    var questionMetric = CreateQuestionMetric(pairModel, exam);

                    var learnList = learningWords;

                    if (!learningWords.Contains(pairModel))
                        learnList = learningWords.Append(pairModel).ToArray();

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

                    var result = exam.Pass(_examService, pairModel, learnList);
                    sw.Stop();
                    questionMetric.ElaspedMs = (int) sw.ElapsedMilliseconds;
                    switch (result)
                    {
                        case ExamResult.Impossible:
                            exam = ExamSelector.GetNextExamFor(i == 0, pairModel);
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
            //--------------------------------------
            
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

        //TODO не понимаю, как работают метрики
        private static QuestionMetric CreateQuestionMetric(UserWordForLearning pairModel, IExam exam)
        {
            var questionMetric = new QuestionMetric
            {
                WordId = pairModel.Id,
                Created=DateTime.Now,
                AggregateScoreBefore = pairModel.AggregateScore, 
                ExamsPassed = pairModel.Examed,
                PassedScoreBefore = pairModel.PassedScore,
                PreviousExam = pairModel.LastExam,
                Type = exam.Name,
            };
            return questionMetric;
        }
    }
}
