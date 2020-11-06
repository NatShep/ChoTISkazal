using System.Collections.Generic;
using Chotiskazal.DAL;
using Chotiskazal.DAL.ModelsForApi;
using Chotiskazal.LogicR.yapi;

namespace Chotiskazal.WebApp.Models
{
    public static class Mapper
    {
        public static TranslationAndContext MapToTranslationAndContext(this WordDictionary wordPair)
        {
            return new TranslationAndContext(wordPair.PairId, wordPair.EnWord, wordPair.RuWord,wordPair.Transcription,wordPair.Phrases.ToArray());
        }
        
        public static PhraseForApi MapToApiPhrase(this Phrase phrase)=> 
            new PhraseForApi
            {
                Origin = phrase.EnPhrase,
                OriginWord = "No origin word",
                Translation = phrase.RuTranslate,
                TranslationWord = "no translation word"
            };
    }
}