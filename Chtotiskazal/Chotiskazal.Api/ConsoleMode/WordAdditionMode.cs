using Chotiskazal.DAL;
using System;
using System.Linq;
using Chotiskazal.Api.Services;


namespace Chotiskazal.Api.ConsoleModes

{
    public class WordAdditionMode : IConsoleMode
    {
     
        private string sourse = "Unknown sourse";

        private readonly AddWordService _addWordService;

        public WordAdditionMode(AddWordService addWordService)
        {
            _addWordService = addWordService;
        }

        public string Name => "Add new words";

        public void Enter(int userId)
        {
            // check Ya status
            var yaStatus = _addWordService.PingYandex();
            if (yaStatus.isYaDicOnline)
                Console.WriteLine("Yandex dic is online");
            else
                Console.WriteLine("Yandex dic is offline");

            if (yaStatus.isYaTransOnline)
                Console.WriteLine("Yandex trans is online");
            else
                Console.WriteLine("Yandex trans is offline");

            // ask word for translation
            while (true)
            {
                Console.Write("Enter [e] for exit or ");
                Console.Write("Enter english word: ");
                string word = Console.ReadLine();
                if (word == "e")
                    break;

                //find word in local dictionary(if not, find it in Ya dictionary)
                var translations = _addWordService.FindInDictionaryWithPhrases(word);
                if (!translations.Any() && yaStatus.isYaDicOnline)
                    translations = _addWordService.TranslateAndAddToDictionary(word);
                if (!translations.Any())
                    Console.WriteLine("No translations found. Check the word and try again");
                
                // choose thr translation
                else
                {
                    Console.WriteLine("e: [back to main menu]");
                    Console.WriteLine("c: [CANCEL THE ENTRY]");
                    int i = 1;
                    foreach (var translation in translations)
                    {
                        if (translation.Phrases.Any())
                            Console.WriteLine($"{i}: {translation.GetTranslations().FirstOrDefault()}\t (+{translation.Phrases.Count})");
                        else
                            Console.WriteLine($"{i}: {translation.GetTranslations().FirstOrDefault()}");
                        i++;
                    }
                    try
                    {
                        var results = ChooseTranslation(translations.ToArray());
                        if (results?.Any() == true)
                        {
                            var count =_addWordService.AddResultToUserCollection(userId, results);
                            Console.WriteLine($"Saved. Translations: {count}");
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                }
            }
        }


        UserWordForLearning[] ChooseTranslation(UserWordForLearning[] translations)
        {
            while (true)
            {
                Console.Write("Choose the word:");
                var res = Console.ReadLine().Trim();
                if (res.ToLower() == "e")
                    throw new OperationCanceledException();
                if (res.ToLower() == "c")
                    return null;

                if (!int.TryParse(res, out var ires))
                {
                    var subItems = res.Split(',');
                    if (subItems.Length > 1)
                    {
                        try
                        {
                            return subItems
                                .Select(s => int.Parse(s.Trim()))
                                .Select(i => translations[i - 1])
                                .ToArray();
                        }
                        catch (Exception e)
                        {
                            continue;
                        }
                    }
                    
//TODO What is this???
                    if (res.Length > 1)
                        return new[]
                        {
                            new UserWordForLearning()
                            {
                                 EnWord = translations[0].EnWord,
                                 UserTranslations = res,
                                 Transcription = translations[0].Transcription,
                                 IsPhrase = false,
                            }
                        };
                    else continue;
                }

                if (ires == 0)
                    return null;
                if (ires > translations.Length || ires < 0)
                    continue;
                return new[] {translations[ires - 1]};
            }
        }
    }
}