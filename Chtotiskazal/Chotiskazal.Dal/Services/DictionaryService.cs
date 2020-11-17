using System.Collections.Generic;
using Chotiskazal.Dal.Repo;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Dal.DAL;
using Chotiskazal.Dal.Enums;

namespace Chotiskazal.Dal.Services
{
    public class DictionaryService
    {
        private readonly DictionaryRepository _dicRepository;

        public DictionaryService(DictionaryRepository repository) => _dicRepository = repository;

        public async Task<int> AddNewWordPairToDictionaryAsync(string enword, string ruword, string transcription,
            TranslationSource sourse)
        {
            var word = new WordDictionary(enword, ruword, transcription, sourse);
            return await _dicRepository.AddWordPairAsync(word);
        }

        public async Task<int[]> AddNewWordPairToDictionaryWithPhrasesAsync(string word, string yandexTranslationText,
            string transcription, TranslationSource yadic, List<Phrase> yaPhrases) =>
            await _dicRepository.AddWordPairWithPhrasesAsync(word, yandexTranslationText, transcription, yadic,
                yaPhrases);

        public async Task<string[]> GetAllTranslationsAsync(string word) =>
            await _dicRepository.GetAllTranslateAsync(word);

       public async Task<WordDictionary[]> GetAllPairsByWordWithPhrasesAsync(string word)=>
            await _dicRepository.GetPairsWithPhrasesByWordOrNullAsync(word);
            
        public async Task<Phrase[]> FindPhrasesBySomeIdsAsync(int[] allPhrasesIdForUser) =>
            await _dicRepository.FindPhrasesBySomeIdsForUserAsync(allPhrasesIdForUser);

    }
}
