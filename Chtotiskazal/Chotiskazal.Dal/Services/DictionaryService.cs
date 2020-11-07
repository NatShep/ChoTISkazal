using Chotiskazal.Dal.Repo;
using Chotiskazal.DAL;
using Chotiskazal.LogicR;
using Chotiskazal.LogicR.yapi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chotiskazal.Dal.Enums;

namespace Chotiskazal.Dal.Services
{
    public class DictionaryService
    {
        private readonly DictionaryRepository _dicRepository;

        public DictionaryService(DictionaryRepository repository) => _dicRepository = repository;

        // Add Words or Phrases to dictionary
        public int AddNewWordPairToDictionary(string enword, string ruword, string transcription,
            TranslationSource sourse)
        {
            var word = new WordDictionary(enword, ruword, transcription, sourse);
            return _dicRepository.AddWordPair(word);
        }

        public int AddPhraseForWordPair(int pairId, string enWord, string ruWord,string enPhrase, string RuTranslate)
        {
            //TODO look if there are some phrases
            //if not, add prases
            // if there is, update phrases
            return _dicRepository.AddPhrase(pairId, enWord, ruWord, enPhrase, RuTranslate);
        }

        public string[] GetAllTranslations(string word) => _dicRepository.GetAllTranslate(word);

        public WordDictionary GetPairWithPhrasesByIdOrNull(int id) => _dicRepository.GetPairWithPhrasesByIdOrNull(id);

        public WordDictionary[] GetAllPairsByWord(string word) => _dicRepository.GetAllWordPairsByWord(word);

        //TODO How is better.
        //This one ore write one SQL querry in new method _dicRepository.GetAllWordPairsWithPhrasesByWord 
        public WordDictionary[] GetAllPairsByWordWithPhrases(string word)
        {
            var wordPairs = GetAllPairsByWord(word);
            foreach (var wordDictionary in wordPairs)
            {
                var phrases = _dicRepository.GetAllPhrasesByPairId(wordDictionary.PairId);
                wordDictionary.Phrases = phrases.ToList();
            }

            return wordPairs;
        }

        public void RemovePhrase(in int phraseId) => _dicRepository.RemovePhrase(phraseId);

        public string GetTranscription(string word) => throw new NotImplementedException();
        public WordDictionary GetPairByIdOrNull(int id) => throw new NotImplementedException();


        public Phrase[] FindSeverealPrasesById(int[] allPhrasesIdForUser) =>
            _dicRepository.FindSeveralPhraseById(allPhrasesIdForUser);
    }
}
