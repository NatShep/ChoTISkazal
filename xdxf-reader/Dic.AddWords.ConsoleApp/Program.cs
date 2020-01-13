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
            var service = new NewWordsService(dictionary, new KnowledgeRepository());
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
                Console.WriteLine("No tranlations found. Check the word and try again");
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
                if(!int.TryParse(res, out var ires))
                    continue;
                if (ires == 0)
                    return null;
                if(ires> match.Translations.Length|| ires<0)
                    continue;
                return match.Translations[ires - 1];
            }
        }
    }
}
