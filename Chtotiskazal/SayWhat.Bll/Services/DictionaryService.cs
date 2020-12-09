using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using SayWhat.Bll.Dto;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Dictionary;
using SayWhat.MongoDAL.Examples;
using DictionaryTranslation = SayWhat.Bll.Dto.DictionaryTranslation;

namespace SayWhat.Bll.Services
{
    public class DictionaryService
    {
        private readonly DictionaryRepo _dicRepository;
        private readonly ExamplesRepo _exampleRepository;

        public DictionaryService(DictionaryRepo repository, ExamplesRepo exampleRepository)
        {
            _dicRepository = repository;
            _exampleRepository = exampleRepository;
        }

        /*public Task AddNewWordPairToDictionaryAsync(
            string enword, 
            string ruword, 
            string transcription, 
            TranslationSource sourse)
        {
            var word = new MongoDAL.Dictionary.DictionaryWord
            {
                Id = ObjectId.GenerateNewId(),
                Language = Language.En,
                Source = sourse,
                Transcription = transcription,
                Word = enword,
                Translations = new[]
                {
                    new MongoDAL.Dictionary.DictionaryTranslation
                    {
                        Word = ruword,
                        Language = Language.Ru,
                        Id = ObjectId.GenerateNewId()
                    }
                }
            };
            return _dicRepository.Add(word);
        }*/

        /*
        public Task AddNewWordPairToDictionaryWithPhrasesAsync(string enword, string ruword,
            string transcription, TranslationSource source, List<Phrase> examples)
        {
            var word = new MongoDAL.Dictionary.DictionaryWord
            {
                Id = ObjectId.GenerateNewId(),
                Language = Language.En,
                Source = source,
                Transcription = transcription,
                Word = enword,
                Translations = new[]
                {
                    new MongoDAL.Dictionary.DictionaryTranslation
                    {
                        Word = ruword,
                        Language = Language.Ru,
                        Id = ObjectId.GenerateNewId(),
                        Examples = examples.Select(p=>new ReferenceToExample
                        {
                            OriginExample =  p.EnPhrase,
                            TranslationExample = p.PhraseRuTranslate
                        }).ToArray()
                    }
                }
            };
            return _dicRepository.Add(word);
        }
        */

        public async Task<string[]> GetAllTranslationWords(string enword)
        {
            var results = await _dicRepository.GetOrDefault(enword);
            if(results==null)
                return new string[0];
            return results.Translations.Select(t => t.Word).ToArray();
        }

        public async Task<IReadOnlyList<DictionaryTranslation>> GetTranslationsWithExamples(string enword)
        {
            var word = await _dicRepository.GetOrDefault(enword);
            if(word==null)
                return new DictionaryTranslation[0];
            
            var result = new List<DictionaryTranslation>();
            foreach (var translation in word.Translations)
            {
                var examples = translation.Examples.Length > 0
                    ? await _exampleRepository.GetAll(translation.Examples.Select(e => e.ExampleId))
                    : new List<Example>();
                
                result.Add(new DictionaryTranslation(
                    word.Word,
                    translation.Word,
                    word.Transcription,
                    word.Language== Language.En? TranslationDirection.EnRu: TranslationDirection.RuEn,
                    word.Source, examples));
            }

            return result;
        }
        
        public async Task<IReadOnlyList<DictionaryTranslation>> GetTranslationsWithoutExamples(string enword)
        {
            var word = await _dicRepository.GetOrDefault(enword);
            if(word==null)
                return new DictionaryTranslation[0];

            return word.Translations
                .Select(translation => new DictionaryTranslation(
                    originText: word.Word, 
                    translatedText: translation.Word, 
                    originTranscription: word.Transcription, 
                    tranlationDirection: word.Language== Language.En? TranslationDirection.EnRu: TranslationDirection.RuEn,
                    source: word.Source, 
                    translation.Examples
                        .Select(e => new Example {Id = e.ExampleId})
                        .ToList()))
                .ToList();
        }
      
       public async Task AddNewWord(DictionaryWord word)
       {
           var allExamples = word.Translations
               .SelectMany(t => t.Examples)
               .Select(e=>e.ExampleOrNull)
               .Where(e=>e!= null);
           await _exampleRepository.Add(allExamples);
           await _dicRepository.Add(word);
       }
    }
}
