using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Chotiskazal.Logic.Services;
using Dic.Logic.yapi;
using Microsoft.AspNetCore.Mvc;

namespace Chotiskazal.RestApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class WordsController : ControllerBase
    {
        private readonly YandexDictionaryApiClient _yandexDictionaryApiClient;
        private readonly YandexTranslateApiClient _yaTransApi;
        private readonly NewWordsService _wordsService;

        public WordsController(NewWordsService wordsService, YandexDictionaryApiClient yaapiClient, YandexTranslateApiClient yaTransApi)
        {
            _yandexDictionaryApiClient = yaapiClient;
            _yaTransApi = yaTransApi;
            _wordsService = wordsService;
        }
        
 //       [Route("health")]

        [HttpGet]
        public async Task<HealthResponse> GetHealth()
        {
            var pingResult =  await _yandexDictionaryApiClient.Ping();
            if (pingResult)
                return new HealthResponse(HealthStatus.Online);
            else
                return new HealthResponse(HealthStatus.Offline);
        }

        [HttpGet("Translation/{word}")]
        public async Task<TranslationResponse> GetTranslation(string word)
        {
            var origin = HttpUtility.UrlDecode(word);

            if (!origin.Contains(' '))
            {
                if (_yandexDictionaryApiClient.IsOnline)
                {
                    var yandexResponse = await _yandexDictionaryApiClient.Translate(origin);
                    var result = yandexResponse.SelectMany(t => t.Tr)
                        .Select(t => new Translation(t.Text, TranslationSource.Yadic)).ToArray();

                    if (result.Any())
                        return new TranslationResponse(origin, result);
                }
            }

            if (!_yaTransApi.IsOnline)
                return TranslationResponse.Empty(origin);

            var res = await _yaTransApi.Translate(origin);

            if (string.IsNullOrWhiteSpace(res))
                return TranslationResponse.Empty(origin);

            return new TranslationResponse(origin, new Translation(res, TranslationSource.Yatrans));
        }
    

        [HttpPut("Words")]
        public void SelectTranslation([FromBody] AddTranslationRequest translationRequest)
        {
            var translations = translationRequest.Translations;
            
            var translationsTexts = translationRequest.Translations.Select(t => t.Text).ToArray();
      //      _wordsService.SaveForExams(translationRequest.Word, null, translationsTexts);
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
        public static TranslationResponse Empty(string origin)=> new TranslationResponse(origin);
        public TranslationResponse(string origin, params Translation[] translations)
        {
            Origin = origin;
            Translations = translations;
        }
        public string Origin { get; }
        public Translation[] Translations { get; }
    }

  
}
