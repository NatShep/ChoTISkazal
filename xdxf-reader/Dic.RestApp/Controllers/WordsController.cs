using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Dic.RestApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WordsController : ControllerBase
    {
        [HttpGet("Health")]
        public HealthResponse GetHealth()
        {
            return new HealthResponse(HealthStatus.Online);
        }

        [HttpGet("Translation/{word}")]
        public TranslationResponse GetTranslation(string word)
        {
            return new TranslationResponse(
                word,
                new Translation("Мими", TranslationSource.Yadic),
                new Translation("Бубубу", TranslationSource.Yadic)
                );
        }
        [HttpPut("Words")]
        public void AddTranslation([FromBody] AddTranslationRequest translationRequest)
        {
            Console.WriteLine("Add translation for " + translationRequest.Word);
            /*
             * request-body: {
                  Word: "War",
                  Translations:[
                    {
                      Text: "Война"
                      Source: "yadic|yatrans|local|manual"
                    },
                    {
                      Text: "Культяпка"
                      Source: "yadic|yatrans|local|manual"
                    }
                  ]  
                }
             */
        }

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

    public enum HealthStatus
    {
        Online,
        Offline
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
