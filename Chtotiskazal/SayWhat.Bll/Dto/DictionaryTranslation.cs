using System.Collections.Generic;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Examples;
// ReSharper disable MemberCanBePrivate.Global

namespace SayWhat.Bll.Dto
{
    public class DictionaryTranslation
    {
        public DictionaryTranslation(
            string enWord, 
            string ruWord, 
            string enTranscription,
            TranslationSource source)
        {
            EnWord = enWord;
            EnTranscription = enTranscription ?? "" ;
            RuWord = ruWord;
            Source = source;
        }
        public DictionaryTranslation(
            string enWord, 
            string ruWord, 
            string enTranscription, 
            TranslationSource source, 
            List<Example> phrases)
            :this(enWord,ruWord,enTranscription,source) => Examples = phrases;
        public string EnWord { get; }
        public string RuWord { get; }
        public string EnTranscription { get; }
        public List<Example> Examples { get; set; } = new List<Example>();
        public TranslationSource Source { get; }

    }
    
    
}
