using Chotiskazal.Dal.Repo;
using Chotiskazal.DAL;
using Chotiskazal.LogicR;
using Chotiskazal.LogicR.yapi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chotiskazal.Dal.Enums;

namespace Chotiskazal.Dal.Services
{
    public class DictionaryService
    {
        private readonly DictionaryRepository _dicRepository;

        public DictionaryService(DictionaryRepository repository) => _dicRepository = repository;

        // Add Words or Phrases to dictionary
        public async Task<int> AddNewWordPairToDictionaryAsync(string enword, string ruword, string transcription,
            TranslationSource sourse)
        {
            var word = new WordDictionary(enword, ruword, transcription, sourse);
            return await _dicRepository.AddWordPairAsync(word);
        }

        public async Task<int> AddPhraseForWordPairAsync(int pairId, string enWord, string ruWord,string enPhrase, string RuTranslate)
        {
            //TODO look if there are some phrases
            //if not, add prases
            // if there is, update phrases
            return await _dicRepository.AddPhraseAsync(pairId, enWord, ruWord, enPhrase, RuTranslate);
        }

        public async Task<string[]> GetAllTranslationsAsync(string word) => await _dicRepository.GetAllTranslateAsync(word);

        public async Task<WordDictionary> GetPairWithPhrasesByIdOrNullAsync(int id) => 
            await _dicRepository.GetPairWithPhrasesByIdOrNullAsync(id);

        public async Task<WordDictionary[]> GetAllPairsByWordAsync(string word) => await _dicRepository.GetAllWordPairsByWordAsync(word);

        //TODO How is better.
        //This one ore write one SQL querry in new method _dicRepository.GetAllWordPairsWithPhrasesByWord 
        public async Task<WordDictionary[]> GetAllPairsByWordWithPhrases(string word)
        {
            var wordPairs =await GetAllPairsByWordAsync(word);
            foreach (var wordDictionary in wordPairs)
            {
                var phrases =await _dicRepository.GetAllPhrasesByPairIdAsync(wordDictionary.PairId);
                wordDictionary.Phrases = phrases.ToList();
            }
            return wordPairs;
        }

        public async Task<Phrase[]> FindSeveralPhrasesByIdAsync(int[] allPhrasesIdForUser) =>
            await _dicRepository.FindSeveralPhraseByIdAsy(allPhrasesIdForUser);
        
        //TODO
        public async Task RemovePhraseAsync(int phraseId) => await _dicRepository.RemovePhraseAsync(phraseId);

        public string GetTranscription(string word) => throw new NotImplementedException();
        public WordDictionary GetPairByIdOrNull(int id) => throw new NotImplementedException();

    }
}
