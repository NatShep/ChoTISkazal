using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Chotiskazal.App;
using Chotiskazal.DAL;
using Chotiskazal.Dal.Enums;
using Chotiskazal.DAL.ModelsForApi;
using Chotiskazal.Dal.Services;
using Chotiskazal.LogicR.yapi;
using static Chotiskazal.LogicR.yapi.MapperForDBModels;
using Chotiskazal.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Chotiskazal.WebApp.Controllers
{
    [Controller]
    public class WordsController : Controller
    {
        private readonly YandexDictionaryApiClient _yandexDictionaryApiClient;
        private readonly UsersPairsService _usersPairsService;
        private readonly UserService _userService;
        private readonly YandexTranslateApiClient _yandexTransApiClient;
        private readonly DictionaryService _dictionaryService;

        public WordsController(YandexDictionaryApiClient yaapiClient, YandexTranslateApiClient yandexTransApiClient
            ,DictionaryService dictionaryService, UsersPairsService usersPairsService, UserService userService)
        {
            _yandexDictionaryApiClient = yaapiClient;
            _yandexTransApiClient = yandexTransApiClient;
            _dictionaryService = dictionaryService;
            _usersPairsService = usersPairsService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<HealthResponse> GetHealthDictionary()
        {
            var pingResult = await _yandexDictionaryApiClient.Ping();
            if (pingResult)
                return new HealthResponse(HealthStatus.Online);
            else
                return new HealthResponse(HealthStatus.Offline);
        }

        [HttpGet]
        public async Task<HealthResponse> GetHealthTranslation()
        {
            var pingResult = await _yandexTransApiClient.Ping();
            if (pingResult)
                return new HealthResponse(HealthStatus.Online);
            else
                return new HealthResponse(HealthStatus.Offline);
        }
        
        [Authorize]
        [HttpGet]
        public IActionResult GetTranslation() => View();
        
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> GetTranslation(string word)
        {
            
            var origin = HttpUtility.UrlDecode(word);

            var translateWithContexts = FindInDictionary(origin);
            
            if (!translateWithContexts.Any())
                translateWithContexts = TranslateByYandex(origin);

            //TODO ViewModel
            // подумать: о структурe viewmodel,  м.б. ее упростить
            // подумать: я создаю viewмодель сразу  в поиске перевода слова, м.б. лучше в другом месте
            return View("SelectTranslation", translateWithContexts);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SelectTranslation(int id)
        {
            var user = _userService.GetUserByLoginOrNull(User.Identity.Name);
            if (user == null)
                return RedirectToAction("Logout", "Account");
            
            var userPair = _usersPairsService.GetPairByDicId(user.UserId, id);
            if (userPair==null)
                _usersPairsService.AddWordToUserCollection(user.UserId, id);
         
            return RedirectToAction("Menu", "Home");

        }
        
        private List<TranslationAndContext> FindInDictionary(string word)
        {
            var pairWords = _dictionaryService.GetAllPairsByWord(word);
          
            // map to viewModel
            var translateWithContexts = new List<TranslationAndContext>();
            foreach (var wordDictionary in pairWords)
                translateWithContexts.Add(wordDictionary.MapToTranslationAndContext());
            
            return translateWithContexts;
        }

        
        public class HealthResponse
        {
            public HealthResponse(HealthStatus status) => Status = status.ToString();
            public string Status { get; }
        }

        //TODO Isolate to Chotiskazal.Api
        private List<TranslationAndContext> TranslateByYandex(string word)
        {
            List<TranslationAndContext> translationsWithContext = new List<TranslationAndContext>();
            
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
                        var dbPhrases = new List<Phrase>();

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
                            _dictionaryService.AddPhraseForWordPair(id, word, null, phrase.EnPhrase, phrase.RuTranslate);
                        }

                        translationsWithContext.Add(new TranslationAndContext(id, word, yandexTranslation.Text,
                            transcription,
                            dbPhrases.ToArray()));
                    }
                    return translationsWithContext;
                }
            }

            try
            {
                var transAnsTask = _yandexTransApiClient.Translate(word);
                transAnsTask.Wait();
                if (!string.IsNullOrWhiteSpace(transAnsTask.Result))
                {
                    //1. Заполняем бд(wordDictionary, фраз нет)
                    var id = _dictionaryService.AddNewWordPairToDictionary(
                        word,
                        transAnsTask.Result,
                        "[]",
                        TranslationSource.Yadic);
                    //2. Дополняем ответ
                    translationsWithContext.Add(new TranslationAndContext(id, word, transAnsTask.Result, "[]",
                        new Phrase[0]));
                }
            }
            catch (Exception e)
            {
                return translationsWithContext;
            }

            return translationsWithContext;
        }
    }
}
