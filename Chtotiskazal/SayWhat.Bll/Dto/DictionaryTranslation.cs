using System.Collections.Generic;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Examples;
// ReSharper disable MemberCanBePrivate.Global

namespace SayWhat.Bll.Dto
{
    public class DictionaryTranslation
    {
        public DictionaryTranslation(
            string originText, 
            string translatedText, 
            string originTranscription,
            TranlationDirection tranlationDirection,
            TranslationSource source)
        {
            OriginText = originText;
            EnTranscription = originTranscription ?? "" ;
            TranslatedText = translatedText;
            TranlationDirection = tranlationDirection;
            Source = source;
        }
        public DictionaryTranslation(
            string originText, 
            string translatedText, 
            string originTranscription, 
            TranlationDirection tranlationDirection,
            TranslationSource source, 
            List<Example> phrases)
            :this(originText,translatedText,originTranscription, tranlationDirection, source) => Examples = phrases;
        public string OriginText { get; }
        public string TranslatedText { get; }
        public TranlationDirection TranlationDirection { get; }
        public string EnTranscription { get; }
        public List<Example> Examples { get; set; } = new List<Example>();
        public TranslationSource Source { get; }

        public DictionaryTranslation GetReversed()
        {
            return new DictionaryTranslation(TranslatedText, OriginText, "", 
                TranlationDirection== TranlationDirection.EnRu? TranlationDirection.RuEn: TranlationDirection.EnRu, Source );
        }

    }
    
    
}
