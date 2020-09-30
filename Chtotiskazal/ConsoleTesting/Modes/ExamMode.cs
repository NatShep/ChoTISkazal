using Chotiskazal.Dal.Services;
using Chotiskazal.DAL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Chotiskazal.LogicR;
using Chotiskazal.Dal;
using Chotiskazal.ConsoleTesting.Exams;

namespace ConsoleTesting.Modes
{
    public class ExamMode : IConsoleMode
    {
        public string Name => "Examination";
        private int NumberOfWordsForExam = 9;
        private UsersWordService _usersWordService;
        private UserService _userService;
        private ExamsAndMetricService _examsAndMetricService;
        private User user;
        private readonly int _maxTranslationSize = 3;
        
        public ExamMode(UsersWordService usersWordService, UserService userService,ExamsAndMetricService examsAndMetricService )
        {
            _usersWordService = usersWordService;
            _examsAndMetricService = examsAndMetricService;
            _userService = userService;
        }
        
        public void Enter(User user)
        {
  
            /*          //Randomization and jobs
            if (RandomTools.Rnd.Next() % 30 == 0)
            {
                //Add phrases with mutual words to vocab
                service.AddMutualPhrasesToVocab(10);
            }
            else
                service.UpdateAgingAndRandomize(50);

    */
            var learningWords = SetExamWords(_usersWordService.GetPairsForLearning(user, NumberOfWordsForExam));          

            Console.WriteLine("Examination: ");
      // пока пусть всегда показывает      if (learningWords.Average(w => w.Metric.PassedScore) <= 4)
            {
                foreach (var word in learningWords.Randomize())
                {
                    Console.WriteLine($"{word.Pair.EnWord}\t\t:{word.TranslationForExam}");
                }
            }
            if (learningWords.Length == 0)
                Console.WriteLine("You haven't words for learning. Add new words to your dictionary.");

            Console.ReadLine();


            /*
            var examsList = new List<PairModel>(learningWords.Length * 4);
            //Every learning word appears in test from 2 to 4 times

            examsList.AddRange(learningWords.Randomize());
            examsList.AddRange(learningWords.Randomize());
            examsList.AddRange(learningWords.Randomize().Where(w => RandomTools.Rnd.Next() % 2 == 0));
            examsList.AddRange(learningWords.Randomize().Where(w => RandomTools.Rnd.Next() % 2 == 0));

            while (examsList.Count > 32)
            {
                examsList.RemoveAt(examsList.Count - 1);
            }

            var delta = Math.Min(7, (32 - examsList.Count));
            PairModel[] testWords = new PairModel[0];
            if (delta > 0)
            {
                var randomRate = 8 + RandomTools.Rnd.Next(5);
                testWords = service.GetPairsForTests(delta, randomRate);
                examsList.AddRange(testWords);
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to start an examination");
            Console.ReadKey();
            Console.Clear();
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

                    var result = exam.Pass(service, pairModel, learnList);

                    sw.Stop();
                    questionMetric.ElaspedMs = (int)sw.ElapsedMilliseconds;
                    switch (result)
                    {
                        case ExamResult.Impossible:
                            exam = ExamSelector.GetNextExamFor(i == 0, pairModel);
                            retryFlag = true;
                            break;
                        case ExamResult.Passed:
                            WritePassed();
                            service.SaveQuestionMetrics(questionMetric);
                            examsCount++;
                            examsPassed++;
                            break;
                        case ExamResult.Failed:
                            WriteFailed();
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
                    lastExamResult = result;

                } while (retryFlag);


                service.RegistrateExam(started, examsCount, examsPassed);

            }

            Console.WriteLine();
            Console.WriteLine($"Test done:  {examsPassed}/{examsCount}");
            foreach (var pairModel in learningWords.Concat(testWords))
            {
                Console.WriteLine(pairModel.OriginWord + " - " + pairModel.Translation + "  (" + pairModel.PassedScore +
                                  ")");
            }

            Console.WriteLine();
            */
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

        private UserPairForExam[] SetExamWords(UserPair[] usersPairs)
        {
            List<UserPairForExam> LearnigWords = new List<UserPairForExam>();
            foreach (var pair in usersPairs)
            {
                var WordForExam = new UserPairForExam(); //дописать как создается
                var translations = _usersWordService.GetAllUserTranslatesForWord(user, pair.PairId).ToArray();

                if (translations.Length <= _maxTranslationSize)
                {
                    LearnigWords.Add(WordForExam);
                    continue;
                }

                var usedTranslations = translations.Randomize().Take(_maxTranslationSize).ToArray();
                WordForExam.SetTranslations(usedTranslations);
                LearnigWords.Add(WordForExam);
               
            }
            return LearnigWords.ToArray();
        }

        private static QuestionMetric CreateQuestionMetric(UserPair userPair, IExam exam)
        {
            var questionMetric = new QuestionMetric
            {
            //    AggregateScoreBefore = pairModel.AggregateScore,
            //    WordId = pairModel.Id,
             //   Created = DateTime.Now,
              //  ExamsPassed = pairModel.Examed,
               // PassedScoreBefore = pairModel.PassedScore,
             //   PhrasesCount = pairModel.Phrases?.Count ?? 0,
              //  PreviousExam = pairModel.LastExam,
                //Type = exam.Name,
               // WordAdded = pairModel.Created
            };
            return questionMetric;


        }
    }
}
