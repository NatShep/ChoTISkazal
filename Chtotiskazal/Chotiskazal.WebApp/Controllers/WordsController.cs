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
        public IActionResult GetTranslation()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> GetTranslation(string word)
        {
            
            var origin = HttpUtility.UrlDecode(word);

            //возвращаю из словаря массив пар слово-перевод, найденных по слову(без фраз)
            var translateWithContexts = FindInDictionary(word);
            
            //если ничего нет, иду в яндекс словарь(там перевожу слово и заношу его сразу в базу)
            if (!translateWithContexts.Any())
                translateWithContexts = TranslateByYandex(word);

            // Передаю этот список во вьюху
            // возможно переводим в модель для вьюхи. Где не обязательно передавать все фразы, а только кол-во фраз.
            // потом подтянем фразы, если будет надо
            // сейчас я создаю модель для вьюхи сразу в поиске перевода слова
            return View("SelectTranslation", translateWithContexts);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SelectTranslation(int id)
        {
            var wordFromDictionary = _dictionaryService.GetPairById(id);
            var user = _userService.GetUserByLogin(User.Identity.Name);
            if (user == null)
                return RedirectToAction("Logout", "Account");
            //добавляем выбранный перевод конкретному юзеру, если его еще нет
            _usersWordService.AddWordToUserCollection(user, id);
            //Если он есть, проверяем на схожесть и при необходимости добавляем данные

            // переходим на меню для работы
            return RedirectToAction("Menu", "Home");

        }
        
        private List<TranslationAndContext> FindInDictionary(string word)
        {
            // возвращаю из бд список пар WordDictionary(Без подтягивания фраз)
            var pairWords = _dictionaryService.GetAllPairsByWord(word);
          
            // перевожу во viewModel
            var translateWithContexts = new List<TranslationAndContext>();
            foreach (var wordDictionary in pairWords)
                translateWithContexts.Add(wordDictionary.MapToTranslationAndContext());
            
            return translateWithContexts;
        }

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
                        
                        translationsWithContext.Add(new TranslationAndContext(id, word, yandexTranslation.Text, transcription,
                            dbPhrases.ToArray()));
                    }
                }
            }

            //Если  в итоге не удалось составить список WordDictionary
            if (!translationsWithContext.Any())
            {
                try
                {
                    var transAnsTask = _yaTransApi.Translate(word);
                    transAnsTask.Wait();

                    if (!string.IsNullOrWhiteSpace(transAnsTask.Result))
                    {
                        //1. Заполняем бд(wordDictionary, фраз нет)
                        var id =_dictionaryService.AddNewWordPairToDictionary(
                            word,
                            transAnsTask.Result,
                            "[]",
                            TranslationSource.Yadic);
                        //2. Дополняем ответ
                        translationsWithContext.Add(new TranslationAndContext(id, word, transAnsTask.Result, "[]", new Phrase[0]));
                    }
                }
                catch (Exception e)
                {
                    return translationsWithContext;
                }
            }
            return translationsWithContext;
        }

        public class HealthResponse
        {
            public HealthResponse(HealthStatus status)
            {
                Status = status.ToString();
            }

            public string Status { get; }
        }
    }
}
