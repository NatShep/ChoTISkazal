using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Dic.Logic.DAL;
using Dic.Logic.Dictionaries;
using Dic.Logic.Services;
using Dic.Logic.yapi;
using Microsoft.AspNetCore.Mvc;

namespace Dic.RestApp.Controllers
{
    [ApiController]
    [Route("/")]
    public class WordsController : ControllerBase
    {
        private YandexApiClient _yandexApiClient;
        private NewWordsService _wordsService;

        public WordsController()
        {
            _yandexApiClient = new YandexApiClient("dict.1.1.20200117T131333Z.11b4410034057f30.cd96b9ccbc87c4d9036dae64ba539fc4644ab33d",
                TimeSpan.FromSeconds(5));
            _wordsService = new NewWordsService(new RuengDictionary(), new WordsRepository());
        }
        [HttpGet("Health")]
        public async Task<HealthResponse> GetHealth()
        {
            var pingResult =  await _yandexApiClient.Ping();
            if (pingResult)
                return new HealthResponse(HealthStatus.Online);
            else
                return new HealthResponse(HealthStatus.Offline);
        }

        [HttpGet("Translation/{word}")]
        public async Task<TranslationResponse> GetTranslation(string word)
        {
            var origin = HttpUtility.UrlDecode(word);

            var yandexResponse = await _yandexApiClient.Translate(origin);
            var result = yandexResponse.SelectMany(t => t.Tr)
                .Select(t => new Translation(t.Text, TranslationSource.Yadic)).ToArray();
            return new TranslationResponse(origin, result);
        }

        [HttpPut("Words")]
        public void SelectTranslation([FromBody] AddTranslationRequest translationRequest)
        {
            var translations = translationRequest.Translations.Select(t => t.Text);
            var translationsString = string.Join(", ", translations);

            _wordsService.SaveForExams(translationRequest.Word, translationsString, null);
        }

    }
    public class HealthResponse
    {
        public HealthResponse(HealthStatus status)
        {
            Status = status.ToString();
        }

        public string Status { get; }
    }
    public enum HealthStatus
    {
        Online,
        Offline
    }
    public class AddTranslationRequest
    {
        public string Word { get; set; }
        public Translation[] Translations { get; set; }
    }
    public class Translation
    {
        public Translation(string text, TranslationSource source)
        {
            Text = text;
            Source = source.ToString();
        }

        public string Text { get; }
        public string Source { get; }
    }

    public enum TranslationSource
    {
        Yadic,
        Yatrans,
        Local,
    }
    public class TranslationResponse
    {
        public TranslationResponse(string origin, params Translation[] translations)
        {
            Origin = origin;
            Translations = translations;
        }
        public string Origin { get; }
        public Translation[] Translations { get; }
    }

  
}
