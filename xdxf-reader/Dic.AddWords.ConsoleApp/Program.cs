using System;
using System.Linq;
using Dic.Logic.DAL;
using Dic.Logic.Dictionaries;
using Dic.Logic.Services;


namespace Dic.AddWords.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            WordsRepository.ApplyMigrations();

            Console.WriteLine("Dic started");
            string path = "T:\\Dictionary\\eng_rus_full.json";
            Console.WriteLine("Loading dictionary");
            var dictionary = Dic.Logic.Dictionaries.Tools.ReadFromFile(path);
            var service = new NewWordsService(dictionary, new WordsRepository());

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine();

                Console.WriteLine("===========================================");

                Console.WriteLine("ESC: Quit");

                Console.WriteLine("1: Enter words");
                Console.WriteLine("2: Examination");
                Console.WriteLine("3: Randomization");
                Console.Write("Choose mode:");

                var val = Console.ReadKey();
                Console.WriteLine();
                switch (val.Key)
                {
                    case ConsoleKey.Escape:
                        return;
                    case ConsoleKey.D1:
                        EnterMode(service);
                        break;
                    case ConsoleKey.D2:
                        ExamMode(service);
                        break;
                    case ConsoleKey.D3:
                        Randomize(service);
                        break;

                }
            }
        }

        static void ExamMode(NewWordsService service)
        {
            Console.WriteLine("Examination");
            var words = service.GetPairsForTest(9);

            var exams = new IExam[]
            {
                new EngChooseExam(),
                new RuChooseExam(),
                new EnTrustExam(),
                new RuTrustExam(),
                new EngWriteExam(), 
                new RuWriteExam() 
            };
            int examsCount = 0;
            int examsPassed = 0;
            DateTime started = DateTime.Now;
            for (int i = 0; i < 3; i++)
            {
                foreach (var pairModel in words.Randomize())
                {
                    Console.WriteLine();
                    var next = Tools.Rnd.Next(exams.Length);
                    var exam = exams[next];

                    bool retryFlag = false;
                    do
                    {
                        retryFlag = false;
                        Console.WriteLine();
                        Console.WriteLine("***** "+exam.Name+ " *****");
                        Console.WriteLine();

                        var result = exam.Pass(service, pairModel, words);
                        switch (result)
                        {
                            case ExamResult.Passed:
                                Console.WriteLine("\r\n[PASSED]");
                                examsCount++;
                                examsPassed++;
                                break;
                            case ExamResult.Failed:
                                Console.WriteLine("\r\n[failed]");
                                examsCount++;
                                break;
                            case ExamResult.Retry:
                                retryFlag = true;
                                break;
                            case ExamResult.Exit: return;
                        }
                    } while (retryFlag);
                }
            }
            
            service.RegistrateExam(started, examsCount, examsPassed);
            
            Console.WriteLine();
            Console.WriteLine($"Test done:  {examsPassed}/{examsCount}");

            foreach (var pairModel in words)
            {
                Console.WriteLine(pairModel.OriginWord+" - "+ pairModel.Translation+"  ("+ pairModel.PassedScore+")");
            }
            Console.WriteLine();

        }


        static void Randomize(NewWordsService service)
        {
            Console.WriteLine("Updating Db");
            service.UpdateAgingAndRandomize();

            var allModels = service.GetAll();
            
            foreach (var pairModel in allModels)
            {
                Console.WriteLine(
                    $"{pairModel.OriginWord} - {pairModel.Translation}   score: {pairModel.PassedScore} / {pairModel.Examed}");
            }
            
            Console.WriteLine($"Has {allModels.Length} models");

            var examSum   = allModels.Sum(a => a.Examed);
            var passedSum = allModels.Sum(a => a.PassedScore);
            var passedAggregatedSum = allModels.Sum(a => Math.Min(1, a.PassedScore/(double)PairModel.MaxExamScore));
            var passedCount = allModels.Count(a => a.PassedScore >= PairModel.MaxExamScore);

            var count = allModels.Length;
            Console.WriteLine($"Knowledge:  {100*passedAggregatedSum/ (double)(count):##.##} %");
            Console.WriteLine($"Known:  {100*passedCount/(double)(count):##.##} %");
            Console.WriteLine($"Failures :  {100*passedSum / (double)(examSum):##.##} %");
        }

        static void EnterMode(NewWordsService service)
        {
            Console.WriteLine("Enter word mode");
            while (true)
            {
                Console.Write("Enter eng word: ");
                string word = Console.ReadLine();
                var translations = service.GetTranlations(word);
                if (translations == null)
                {
                    Console.WriteLine("No translations found. Check the word and try again");
                }
                else
                {
                    Console.WriteLine($"[{translations.Transcription}]");
                    Console.WriteLine("0: [CANCEL THE ENTRY]");
                    int i = 1;
                    foreach (var translation in translations.Translations)
                    {
                        Console.WriteLine($"{i}: {translation}");
                        i++;
                    }

                    var result =  ChooseTranslation(translations);
                    if(result==null)
                    {
                    }
                    else
                    {
                        service.SaveForExams(translations.Origin, result, translations.Transcription);
                        Console.WriteLine("Saved.");
                    }
                }
            }
        }

        static string ChooseTranslation(DictionaryMatch match)
        {
            while (true)
            {
                Console.Write("Choose the word:");
                var res = Console.ReadLine();
                if (!int.TryParse(res, out var ires) )
                {
                    if (res.Length > 1)
                        return res;
                    else continue;
                }
                if (ires == 0)
                    return null;
                if(ires> match.Translations.Length|| ires<0)
                    continue;
                return match.Translations[ires - 1];
            }
        }
    }
}
