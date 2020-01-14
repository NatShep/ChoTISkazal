using System;
using Dic.Logic.DAL;
using Dic.Logic.Dictionaries;
using Dic.Logic.Services;


namespace Dic.AddWords.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Dic started");
            string path = "T:\\Dictionary\\eng_rus_full.json";
            Console.WriteLine("Loading dictionary");

            var dictionary = Tools.ReadFromFile(path);
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
                        RandomizationMode(service);
                        break;

                }
            }
        }

        static void ExamMode(NewWordsService service)
        {
            Console.WriteLine("Examination");
            var words = service.GetPairsForTest(10);
            foreach (var pairModel in words)
            {
               if(!EngTrustExam(pairModel, service))
                   return;
            }
            foreach (var pairModel in words)
            {
                if (!RuTrustExam(pairModel, service))
                    return;
            }
        }
        static bool RuTrustExam(PairModel model, NewWordsService service)
        {
            Console.WriteLine("Ru Trust exam");
            Console.WriteLine("=====>   "+ model.Translation+"    <=====" );
            Console.WriteLine("Press any key to see the translation...");
            Console.ReadKey();
            while (true)
            {

                Console.WriteLine("Translation is " + model.OriginWord + ". Did you guess?");
                Console.WriteLine("[Y]es [N]o [E]xit");
                var answer = Console.ReadKey();
                switch (answer.Key)
                {
                    case ConsoleKey.Y:
                        service.RegistrateSuccess(model);
                        return true;
                    case ConsoleKey.N:
                        service.RegistrateFailure(model);
                        return true;
                    case ConsoleKey.Escape:
                        return false;
                }

            }
        }
        static bool EngTrustExam(PairModel model, NewWordsService service)
        {
            Console.WriteLine("Trust exam");
            Console.WriteLine("=====>   " + model.OriginWord + "    <=====");
            Console.WriteLine("Press any key to see the translation...");
            Console.ReadKey();
            while (true)
            {

                Console.WriteLine("Translation is \r\n" + model.Translation + "\r\n Did you guess?");
                Console.WriteLine("[Y]es [N]o [E]xit");
                var answer = Console.ReadKey();
                switch (answer.Key)
                {
                    case ConsoleKey.Y:
                        service.RegistrateSuccess(model);
                        return true;
                    case ConsoleKey.N:
                        service.RegistrateFailure(model);
                        return true;
                    case ConsoleKey.Escape:
                        return false;
                }

            }
        }
        static void RandomizationMode(NewWordsService service)
        {
            Console.WriteLine("Updating Db");
            service.UpdateAgingAndRandomize();

            var allModels = service.GetAll();
            Console.WriteLine($"Has {allModels.Length} models");
            foreach (var pairModel in allModels)
            {
                Console.WriteLine(
                    $"{pairModel.OriginWord} - {pairModel.Translation}   score: {pairModel.AggregateScore}");
            }
        }

        static void EnterMode(NewWordsService service)
        {
            Console.WriteLine("Enter word mode");
            while (true)
            {
                EnterWord(service);
            }
        }
        static void EnterWord(NewWordsService service)
        {
            Console.Write("Enter eng word: ");
            string word = Console.ReadLine();
            var translations = service.GetTranlations(word);
            if (translations == null)
            {
                Console.WriteLine("No translations found. Check the word and try again");
                return;
            }

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
                return;
            service.SaveForExams(translations.Origin, result, translations.Transcription);
            Console.WriteLine("Saved.");
            return;
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
