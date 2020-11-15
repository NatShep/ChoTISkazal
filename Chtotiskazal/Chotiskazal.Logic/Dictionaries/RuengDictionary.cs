using System;
using System.Collections.Generic;
using System.Linq;

namespace Dic.Logic.Dictionaries
{
    public class RuEngDictionary
    {
        readonly Dictionary<string, DictionaryMatch> _dictionary = new Dictionary<string, DictionaryMatch>(); 
        public void Add(string originWord, string transcription, string[] translations)
        {
            if (!_dictionary.TryAdd(originWord.ToLower(), new DictionaryMatch(originWord, transcription,
                translations.OrderBy(t => t.Length).ToArray())))
            {
               // Console.WriteLine("Same word: "+ originWord);
            }
        }

        public DictionaryMatch GetOrNull(string originWord) 
            => _dictionary.TryGetValue(originWord, out var match) ? match : null;
    }

    public class DictionaryMatch
    {
        public DictionaryMatch(string origin, string transcription, string[] translations)
        {
            Origin = origin;
            Transcription = transcription;
            Translations = translations;
        }

        public string Origin { get; set; }
        public string Transcription { get; set; }
        public string[] Translations { get; set; }
    }
}
