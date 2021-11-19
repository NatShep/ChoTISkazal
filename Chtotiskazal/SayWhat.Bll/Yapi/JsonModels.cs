using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Examples;

namespace SayWhat.Bll.Yapi
{
    public class YapiDicAnswer
    {
        [JsonPropertyName("def")]
        public YaDefenition[] Defenitions { get; set; }
    }

    public class YaDefenition
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
        [JsonPropertyName("pos")]
        public string Pos { get; set; }
        [JsonPropertyName("ts")]
        public string Ts { get; set; }
        [JsonPropertyName("tr")]
        public YaTranslation[] Tr { get; set; }

    }

    public class YaTranslation
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
        [JsonPropertyName("pos")]
        public string Pos { get; set; }
        [JsonPropertyName("gen")]
        public string Gen { get; set; }
        [JsonPropertyName("syn")]
        public YaSynonim[] Syn { get; set; }
        [JsonPropertyName("mean")]
        public YaJustText[] Mean { get; set; }
        [JsonPropertyName("ex")]
        public YaExample[] Ex { get; set; }
        public List<Example> GetPhrases(string word)
        {
            var phrases = new List<Example>();
            if (this.Ex == null) 
                return phrases;
            phrases.AddRange(this.Ex.Select(example => new Example
            {
                OriginWord = word, 
                OriginPhrase = example.Text, 
                TranslatedPhrase = example.Tr.FirstOrDefault()?.Text, 
                TranslatedWord = this.Text,
                Direction = TranslationDirection.EnRu,
            }));
            return phrases;
        }

    }

    public class YaSynonim
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
        [JsonPropertyName("зщы")]
        public string Pos { get; set; }
        [JsonPropertyName("gen")]
        public string Gen { get; set; }
    }

    public class YaJustText
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }

    public class YaExample
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
        [JsonPropertyName("tr")]
        public YaJustText[] Tr { get; set; }
    }
    
}

