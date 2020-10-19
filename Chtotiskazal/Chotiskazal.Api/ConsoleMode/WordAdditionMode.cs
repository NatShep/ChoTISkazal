using Chotiskazal.Dal.Services;
using Chotiskazal.DAL;
using Chotiskazal.LogicR;
using Chotiskazal.LogicR.yapi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Chotiskazal.Api.Services;
using Chotiskazal.ConsoleTesting;
using Chotiskazal.DAL.ModelsForApi;


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

            // write wotd for translation
            while (true)
            {
                Console.Write("Enter [e] for exit or ");
                Console.Write("Enter english word: ");
                string word = Console.ReadLine();
                if (word == "e")
                    break;

                //find word in local dictionary(if nor, find it in Ya dictionary
                var translations = _addWordService.FindInDictionaryWithPhrases(word);
                if (!translations.Any() && yaStatus.isYaDicOnline)
                    translations = _addWordService.TranslateAndAddToDb(word);
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
                            Console.WriteLine($"{i}: {translation.RuWord}\t (+{translation.Phrases.Length})");
                        else
                            Console.WriteLine($"{i}: {translation.RuWord}");
                        i++;
                    }
                    try
                    {
                        var results = ChooseTranslation(translations.ToArray());
                        if (results?.Any() == true)
                        {
                            //adding to user collection, if there is no this word
                            var idsInDb = results.Select(t => t.IdInDB).ToArray();
                            var userTranslations = results.Select(t => t.RuWord).ToArray();
                            var allPhrases = results.SelectMany(t => t.Phrases).ToArray();

                            foreach (var id in idsInDb)
                                 _addWordService.AddPairToUserCollection(userId, id);

                            Console.WriteLine(
                                $"Saved. Tranlations: {userTranslations.Length}, Phrases: {allPhrases.Length}");
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                }
            }
        }


        TranslationAndContext[] ChooseTranslation(TranslationAndContext[] translations)
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

                    if (res.Length > 1)
                        return new[]
                        {
                            new TranslationAndContext(translations[0].IdInDB, translations[0].EnWord, res, translations[0].Transcription,
                                new Phrase[0])
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