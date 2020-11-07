using Chotiskazal.LogicR.yapi;
using System.Collections.Generic;
using Chotiskazal.Dal.Enums;

namespace Chotiskazal.DAL
{
    public class WordDictionary
    {

        public WordDictionary() { }

        public WordDictionary(string enWord, string translation, string transcription, TranslationSource sourse)
        {
            EnWord = enWord;
            Transcription = transcription;
            RuWord = translation;
            Sourse = sourse;
        }
        public WordDictionary(string enWord, string translation, string transcription, TranslationSource sourse, List<Phrase> phrases)
            :this(enWord,translation,transcription,sourse) => Phrases = phrases;
            
        public int PairId { get; set; }
        public string EnWord { get; set; }

        //for one Word has one Translation
        //we can use composite key(EnWord+RuWord)   
        public string RuWord { get; set; }
        public string Transcription { get; set; }

        public List<Phrase> Phrases { get; set; } = new List<Phrase>();
        public TranslationSource Sourse { get; set; }

    }
    
    
}
