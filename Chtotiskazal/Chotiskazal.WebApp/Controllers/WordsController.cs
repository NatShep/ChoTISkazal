using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Chotiskazal.App;
using Chotiskazal.DAL;
using Chotiskazal.Dal.Enums;
using Chotiskazal.Dal.Services;
using Chotiskazal.LogicR.yapi;
using static Chotiskazal.LogicR.yapi.MapperForDBModels;
using Chotiskazal.WebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace Chotiskazal.WebApp.Controllers
{
    [Controller]
    public class WordsController : Controller
    {
        private readonly YandexDictionaryApiClient _yandexDictionaryApiClient;
        private readonly YandexTranslateApiClient _yaTransApi;
        private readonly DictionaryService _dictionaryService;

        public WordsController(YandexDictionaryApiClient yaapiClient, YandexTranslateApiClient yaTransApi,DictionaryService dictionaryService)
        {
            _yandexDictionaryApiClient = yaapiClient;
            _yaTransApi = yaTransApi;
            _dictionaryService = dictionaryService;
        }

        [HttpGet]
        public async Task<HealthResponse> GetHealth()
        {
            var pingResult = await _yandexDictionaryApiClient.Ping();
            if (pingResult)
                return new HealthResponse(HealthStatus.Online);
            else
                return new HealthResponse(HealthStatus.Offline);
        }

        [HttpGet]
        public IActionResult GetTranslation()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetTranslation(string word)
        {
            var origin = HttpUtility.UrlDecode(word);

            //возвращаю из словаря массив пар слово-перевод, найденных по слову(без фраз)
            var wordsFromDictionary = FindInDictionary(word);

            //преобразую его в модель для вида
            var translateWithContexts = new List<TranslationAndContext>();
            foreach (var wordDictionary in wordsFromDictionary)
                translateWithContexts.Add(wordDictionary.MapToTranslationAndContext());

            //если ничего нет, иду в яндекс словарь(там перевожу слово и заношу его сразу в базу)
            if (!translateWithContexts.Any())
                translateWithContexts = TranslateByYandex(word);

            // Передаю этот список во вьюху
            // возможно переводим в модель для вьюхи. Где не обязательно передавать все фразы, а только кол-во фраз.
            // потом подтянем фразы, если будет надо
            // сейчас я создаю модель для вьюхи сразу в поиске перевода слова
            return View("ShowTranslate", translateWithContexts);
        }

        public IActionResult SelectTranslation(TranslationAndContext[] translates)
        {
            return View(translates);
        }

        [HttpPut]
        public void SelectTranslation([FromBody] AddTranslationRequest translationRequest)
        { 
            var translations = translationRequest.Translations;
            var translationsTexts = translationRequest.Translations.Select(t => t.Text).ToArray();
            //      _wordsService.SaveForExams(translationRequest.Word, null, translationsTexts);
        }
        
        private WordDictionary[] FindInDictionary(string word)
        {
            // возвращаю из бд список пар WordDictionary(Без подтягивания фраз)
            var pairWords = _dictionaryService.GetWordPairOrNullByWord(word);
            if (pairWords==null)
                throw new Exception("error in finding words in dictionary");
            return pairWords;
        }

        private List<TranslationAndContext> TranslateByYandex(string word)
        {
            List<TranslationAndContext> translations = new List<TranslationAndContext>();
            List<WordDictionary> wordsForDictionary = new List<WordDictionary>();

            if (!word.Contains(' '))
            {
                Task<YaDefenition[]> task = null;
                if (_yandexDictionaryApiClient.IsOnline)
                    task = _yandexDictionaryApiClient.Translate(word);

                task?.Wait();

                //Создаем из ответа(если он есть)  TranslationAndContext или WordDictionary?

                if (task?.Result?.Any() == true)
                {
                    var variants = task.Result.SelectMany(r => r.Tr);
                    var transcription = task.Result.Select(r => r.Ts).FirstOrDefault();
                    
                    foreach (var yandexTranslation in variants)
                    {
                        var yaPhrases = yandexTranslation.GetPhrases(word);
                        var dbPhrases= new List<Phrase>();
                        
                        //Заполняем бд(wordDictionary и фразы)
                        var id = _dictionaryService.AddNewWordPairToDictionary(
                            word,
                            yandexTranslation.Text,
                            transcription,
                            TranslationSource.Yadic);

                        foreach (var yaPhrase in yaPhrases)
                        {
                            var phrase = yaPhrase.MapToDbPhrase();
                            dbPhrases.Add(phrase);
                            _dictionaryService.AddPhraseForWordPair(id, phrase.EnPhrase, phrase.RuTranslate);
                        }
                        
                        translations.Add(new TranslationAndContext(word, yandexTranslation.Text, transcription,
                            dbPhrases.ToArray()));
                    }
                }

                /*        else //Если ответа нет, не понимаю, что делает он...???
                        {
                            var dictionaryMatch = service.GetTranslations(word);
                            if (dictionaryMatch != null)
                            {
                                translations.AddRange(
                                    dictionaryMatch.Translations.Select(t =>
                                        new TranslationAndContext(dictionaryMatch.Origin, t, dictionaryMatch.Transcription,
                                            new Phrase[0])));
                            }
                        }
            */
            }

            //Если  в итоге не удалось составить список WordDictionary
            if (!translations.Any())
            {
                try
                {
                    var transAnsTask = _yaTransApi.Translate(word);
                    transAnsTask.Wait();

                    //1. Заполняем бд(wordDictionary, фраз нет)
                    if (!string.IsNullOrWhiteSpace(transAnsTask.Result))
                    {
                        _dictionaryService.AddNewWordPairToDictionary(
                            word,
                            transAnsTask.Result,
                            "[]",
                            TranslationSource.Yadic);

                        translations.Add(new TranslationAndContext(word, transAnsTask.Result, "[]", new Phrase[0]));
                    }
                }
                catch (Exception e)
                {
                    return translations;
                }
            }

            //Если удалось, то 
            //2. Возвращаем список WordDictionary
            return translations;
        }

        public class HealthResponse
        {
            public HealthResponse(HealthStatus status)
            {
                Status = status.ToString();
            }

            public string Status { get; }
        }

        public class AddTranslationRequest
        {
            public string Word { get; set; }
            public Translation[] Translations { get; set; }
        }
    }
}
