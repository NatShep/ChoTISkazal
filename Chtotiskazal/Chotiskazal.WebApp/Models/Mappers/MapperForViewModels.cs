using System.Collections.Generic;
using Chotiskazal.App;
using Chotiskazal.DAL;
using Chotiskazal.LogicR.yapi;

namespace Chotiskazal.WebApp.Models
{
    public static class MapperForViewModels
    {
        public static TranslationAndContext MapToTranslationAndContext(this WordDictionary wordPair)
        {
            return new TranslationAndContext(wordPair.EnWord, wordPair.RuWord,wordPair.Transcription,wordPair.Phrases);
        }
    }
}