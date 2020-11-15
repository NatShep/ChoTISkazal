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


namespace Chotiskazal.Api.ConsoleModes

{
    public class WordAdditionMode : IConsoleMode
    {
        private readonly YandexDictionaryApiClient _yapiDicClient;
        private readonly YandexTranslateApiClient _yapiTransClient;
        private readonly UsersWordService _usersWordService;
        private readonly DictionaryService _dictionaryService;
        private string sourse = "Unknown sourse";

        private readonly AddWordService _addWordService;

        public WordAdditionMode(YandexTranslateApiClient yapiTranslateApiClient,
            YandexDictionaryApiClient yandexDictionaryApiClient, UsersWordService usersWordService,
            DictionaryService dictionaryService, AddWordService addWordService)
        {
            _yapiDicClient = yandexDictionaryApiClient;
            _yapiTransClient = yapiTranslateApiClient;
            _usersWordService = usersWordService;
            _dictionaryService = dictionaryService;
            _addWordService = addWordService;
        }

        public string Name => "Add new words";

        public void Enter(int userId)
        {
            var yaStatus = _addWordService.PingYandex();
            
            if (yaStatus.isYaDicOnline)
                Console.WriteLine("Yandex dic is online");
            else
                Console.WriteLine("Yandex dic is offline");

            if (yaStatus.isYaTransOnline)
                Console.WriteLine("Yandex trans is online");
            else
                Console.WriteLine("Yandex trans is offline");

            while (true)
            {
                Console.Write("Enter [e] for exit or ");
                Console.Write("Enter english word: ");
                string word = Console.ReadLine();
                if (word == "e")
                    break;

                Task<YaDefenition[]> task = null;
//                List<TranslationAndContext> translations = new List<TranslationAndContext>();

                if (yaStatus.isYaDicOnline)
                    _addWordService.Translate(word);
               
                
                task = _addWordService.Translate(word);
                task?.Wait();

                List<TranslationAndContext> translations = new List<TranslationAndContext>();
                if (task?.Result?.Any() == true)
                {
                    sourse = "Yandex Dictionary";
                    var variants = task.Result.SelectMany(r => r.Tr);

                    foreach (var yandexTranslation in variants)
                    {
                        var phrases = yandexTranslation.GetPhrases(word);
                        translations.Add(new TranslationAndContext(word, yandexTranslation.Text, yandexTranslation.Pos,
                            new Phrase[0])); //добавить переведенные фразы вместо Phrase[0]
                    }
                }
             
                if (!translations.Any())
                {
                    try
                    {
                        var transAnsTask = _yapiTransClient.Translate(word);
                        transAnsTask.Wait();

                        if (string.IsNullOrWhiteSpace(transAnsTask.Result))
                        {
                            Console.WriteLine("No translations found. Check the word and try again");
                        }
                        else
                        {
                            translations.Add(new TranslationAndContext(word, transAnsTask.Result, null, new Phrase[0]));
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("No translations found. Check the word and try again");
                    }
                }

                if (translations.Any())
                {
                    Console.WriteLine("e: [back to main menu]");
                    Console.WriteLine("c: [CANCEL THE ENTRY]");
                    int i = 1;
                    foreach (var translation in translations)
                    {
                        if (translation.Phrases.Any())
                            Console.WriteLine($"{i}: {translation.Translation}\t (+{translation.Phrases.Length})");
                        else
                            Console.WriteLine($"{i}: {translation.Translation}");
                        i++;
                    }

                    try
                    {
                        var results = ChooseTranslation(translations.ToArray());
                        if (results?.Any() == true)
                        {
                            var userTranslations = results.Select(t => t.Translation).ToArray();
                            var allPhrases = results.SelectMany(t => t.Phrases).ToArray();
                            var allMeanings = translations.Select(t => t.Translation).ToArray();


                            //TODO Adding To dictionary

                            /*    _dictionaryService.AddNewWordToDictionary(
                                enword: word,
                                transcription: translations[0].Transcription,
                                ruword: allMeanings,
                                phrases: allPhrases,
                                sourse: sourse);
                            _usersWordService.SavePairToUser(
                                userId: user.UserId,
                                word: word,
                                userTranslations: userTranslations,
                                IsPhrase: false);
                           */

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
                            new TranslationAndContext(translations[0].Origin, res, translations[0].Transcription,
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