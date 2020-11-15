 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using Dic.Logic.DAL;

namespace Dic.Logic.yapi
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
        public Translation[] Tr { get; set; }

    }

    public class Translation
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
        [JsonPropertyName("pos")]
        public string Pos { get; set; }
        [JsonPropertyName("gen")]
        public string Gen { get; set; }
        [JsonPropertyName("syn")]
        public Synonim[] Syn { get; set; }
        [JsonPropertyName("mean")]
        public JustText[] Mean { get; set; }
        [JsonPropertyName("ex")]
        public Example[] Ex { get; set; }

        public List<Phrase> GetPhrases(string word)
        {
            List<Phrase> phrases = new List<Phrase>();
            if (this.Ex != null)
            {
                foreach (var example in this.Ex)
                {
                    var phrase = new Phrase
                    {
                        Created = DateTime.Now,
                        OriginWord = word,
                        Origin = example.Text,
                        //Translation = example.Tr.FirstOrDefault()?.Text,
                        TranslationWord = this.Text,
                    };
                    phrases.Add(phrase);
                }
            }

            return phrases;
        }
    }

    public class Synonim
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
        [JsonPropertyName("зщы")]
        public string Pos { get; set; }
        [JsonPropertyName("gen")]
        public string Gen { get; set; }
    }

    public class JustText
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }

    public class Example
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
        [JsonPropertyName("tr")]
        public JustText[] Tr { get; set; }
    }
}

