using Chotiskazal.DAL;
using Chotiskazal.DAL.ModelsForApi;

namespace Chotiskazal.Api.Models
{
    public static class MapperForApiModels
    {
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