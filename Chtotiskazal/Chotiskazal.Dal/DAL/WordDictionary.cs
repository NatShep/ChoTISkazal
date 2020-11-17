using System.Collections.Generic;
using Chotiskazal.Dal.Enums;

// ReSharper disable MemberCanBePrivate.Global

namespace Chotiskazal.Dal.DAL
{
    public class WordDictionary
    {
        public WordDictionary() { }

        public WordDictionary(string enWord, string translation, string transcription, TranslationSource source)
        {
            EnWord = enWord;
            Transcription = transcription ?? "[]" ;
            RuWord = translation;
            Source = source;
        }
        public WordDictionary(string enWord, string translation, string transcription, TranslationSource source, List<Phrase> phrases)
            :this(enWord,translation,transcription,source) => Phrases = phrases;

        public int PairId { get; set; } 
        public string EnWord { get; }
        public string RuWord { get; }
        public string Transcription { get; }

        public List<Phrase> Phrases { get; set; } = new List<Phrase>();
        public TranslationSource Source { get; }

    }
    
    
}
