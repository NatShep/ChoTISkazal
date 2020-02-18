using System;
using System.Collections.Generic;
using System.Linq;
using Chotiskazal.Logic.Services;
using Dic.Logic.DAL;
using Dic.Logic.yapi;

namespace Chotiskazal.App.Modes
{
    public class AddPhraseToWordsMode: IConsoleMode
    {
        private readonly YandexDictionaryApiClient _client;

        public AddPhraseToWordsMode(YandexDictionaryApiClient client)
        {
            _client = client;
        }
        public string Name => "Update words";
        public void Enter(NewWordsService service)
        {
            int phraselessCount = 0;
            var allWords = service.GetAll();
            int newPhrases = 0;
            foreach (var pairModel in allWords)
            {
                if (pairModel.Revision >= 1) continue;
                
                var  translateTask = _client.Translate(pairModel.OriginWord);
                translateTask.Wait();
                var yaTranslations = translateTask.Result.SelectMany(r => r.Tr).Select(s=>s.Text.Trim().ToLower());
                var originTranlations = pairModel.Translation.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim().ToLower());

                pairModel.AllMeanings = string.Join(";;", yaTranslations.Union(originTranlations).Distinct());

                var withPhrases = service.GetOrNullWithPhrases(pairModel.OriginWord);
                if (!withPhrases.Phrases.Any() || (withPhrases.Phrases.Count == 1 && withPhrases.Phrases[0].IsEmpty))
                {
                    var allPhrases =   translateTask.Result.SelectMany(r => r.Tr).SelectMany(r => r.GetPhrases(pairModel.OriginWord)).ToList();
                    foreach (var phrase in allPhrases)
                    {
                        service.Add(phrase);
                        newPhrases++;
                    }

                    if (!allPhrases.Any())
                        phraselessCount++;
                }
                pairModel.Revision = 1;
                service.UpdateRatings(pairModel);
                Console.Write(".");
            }
            Console.WriteLine("new phrases:" + newPhrases);
            Console.WriteLine("Phraseless count:"+ phraselessCount);
            Console.WriteLine("withPhrases:" + (allWords.Length-phraselessCount));

        }
    }
}