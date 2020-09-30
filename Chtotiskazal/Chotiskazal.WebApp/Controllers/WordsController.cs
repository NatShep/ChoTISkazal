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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Chotiskazal.WebApp.Controllers
{
    [Controller]
    public class WordsController : Controller
    {
        private readonly YandexDictionaryApiClient _yandexDictionaryApiClient;
        private readonly UsersWordService _usersWordService;
        private readonly UserService _userService;
        private readonly YandexTranslateApiClient _yaTransApi;
        private readonly DictionaryService _dictionaryService;

        public WordsController(YandexDictionaryApiClient yaapiClient, YandexTranslateApiClient yaTransApi
            ,DictionaryService dictionaryService, UsersWordService usersWordService, UserService userService)
        {
            _yandexDictionaryApiClient = yaapiClient;
            _yaTransApi = yaTransApi;
            _dictionaryService = dictionaryService;
            _usersWordService = usersWordService;
            _userService = userService;
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

            //TODO
            // подумать: о структурe viewmodel,  м.б. ее упростить
            // подумать: я создаю viewмодель сразу  в поиске перевода слова, м.б. лучше в другом месте
            //TODO
            return View("SelectTranslation", translateWithContexts);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SelectTranslation(int id)
        {
            var user = _userService.GetUserByLoginOrNull(User.Identity.Name);
            if (user == null)
                return RedirectToAction("Logout", "Account");
            
            var userPair = _usersWordService.GetPairByDicId(user, id);
            if (userPair==null)
                _usersWordService.AddWordToUserCollection(user, id);
         
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

        //TODO
        //Isolate to Chotiskazal.Api
        //TODO
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
                            _dictionaryService.AddPhraseForWordPair(id, phrase.EnPhrase, phrase.RuTranslate);
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
                var transAnsTask = _yaTransApi.Translate(word);
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

        //TODO
        //Isolate to Chotiskazal.Api
        //TODO
        public class HealthResponse
        {
            public HealthResponse(HealthStatus status) => Status = status.ToString();
            public string Status { get; }
        }
    }
}
