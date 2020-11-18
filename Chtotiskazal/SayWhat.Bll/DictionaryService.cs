using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Dal.DAL;
using MongoDB.Bson;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Dictionary;

namespace Chotiskazal.Dal.Services
{
    public class DictionaryService
    {
        private readonly DictionaryRepo _dicRepository;

        public DictionaryService(DictionaryRepo repository) => _dicRepository = repository;

        public Task AddNewWordPairToDictionaryAsync(
            string enword, 
            string ruword, 
            string transcription, 
            TranslationSource sourse)
        {
            var word = new DictionaryWord
            {
                Id = ObjectId.GenerateNewId(),
                Language = Language.En,
                Source = sourse,
                Transcription = transcription,
                Word = enword,
                Translations = new[]
                {
                    new DictionaryTranslation
                    {
                        Word = ruword,
                        Language = Language.Ru,
                        Id = ObjectId.GenerateNewId()
                    }
                }
            };
            return _dicRepository.Add(word);
        }

        public Task AddNewWordPairToDictionaryWithPhrasesAsync(string enword, string ruword,
            string transcription, TranslationSource source, List<Phrase> examples)
        {
            var word = new DictionaryWord
            {
                Id = ObjectId.GenerateNewId(),
                Language = Language.En,
                Source = source,
                Transcription = transcription,
                Word = enword,
                Translations = new[]
                {
                    new DictionaryTranslation
                    {
                        Word = ruword,
                        Language = Language.Ru,
                        Id = ObjectId.GenerateNewId(),
                        Examples = examples.Select(p=>new DictionaryExample
                        {
                            OriginExample =  p.EnPhrase,
                            TranslationExample = p.PhraseRuTranslate
                        }).ToArray()
                    }
                }
            };
            return _dicRepository.Add(word);
        }

        public async Task<string[]> GetAllTranslationsAsync(string enword)
        {
            var results = await _dicRepository.GetOrDefault(enword);
            if(results==null)
                return new string[0];
            return results.Translations.Select(t => t.Word).ToArray();
        }

        public async Task<WordDictionary[]> GetAllPairsByWordWithPhrasesAsync(string enword)
        {
            var word = await _dicRepository.GetOrDefault(enword);
            if(word==null)
                return new WordDictionary[0];
            return word.Translations.Select(t => new WordDictionary(
                word.Word,
                t.Word,
                t.Transcription,
                word.Source,
                t.Examples.Select(e => new Phrase(e.OriginExample, e.TranslationExample)).ToList()))
                .ToArray();
        }
       // public async Task<Phrase[]> FindPhrasesBySomeIdsAsync(int[] allPhrasesIdForUser) =>
       //     await _dicRepository.FindPhrasesBySomeIdsForUserAsync(allPhrasesIdForUser);
    }
}
