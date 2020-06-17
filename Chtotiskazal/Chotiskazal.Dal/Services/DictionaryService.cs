using Chotiskazal.Dal.Repo;
using Chotiskazal.DAL;
using Chotiskazal.LogicR;
using Chotiskazal.LogicR.yapi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chotiskazal.Dal.Services
{
    class DictionaryService
    {
        private readonly DictionaryRepository _dicRepository;

        public DictionaryService( DictionaryRepository repository)
        {
            _dicRepository = repository;
        }

        // Add Words or Phrases to dictionary
        public void AddNewWordPairToDictionary(string originword, string translation, string transcription, string sourse)
        {

        }
        public void AddPhrasesForWordPair(int pairId, Phrase[] phrases)
        {
            //look if there are some phrases
            //if not, add prases
            //if there is, update phrases
            _dicRepository.AddPhrases(_dicRepository.GetWordPairOrNull(pairId), phrases);
        }
        public void AddWordTranslatesAndPhrasesToDictioary(string word, string transcription, string[] translations, string[] allMeanings, Phrase[] phrases = null)
        {
            var alreadyExists = _dicRepository.GetWordPairOrNullByWord(word);
        }
        public void AddWordTranslatesAndPhrasesToDictioary(YaDefenition yandexDTO)
        {

        }

        // Get Translations or Phrases of word
        public string[] GetTranslations(string word)
        {
            return _dicRepository.GetAllTranslate(word);
        }
        public Phrase[] GetAllPhrasesByWord(string word) => _dicRepository.GetAllPhrasesForWordOrNull(word);
        public Phrase[] GetAllPhrasesByWordPair(int WordPairId)
        {
            return new Phrase[0];
        }
        public Phrase[] GetAllPhrasesByWordPair(WordPairDictionary wordPair)
        {
            return new Phrase[0];
        }
    
        
        // Get All or single
        public WordPairDictionary[] GetAllWordPair() => _dicRepository.GetAllWordPairs();
        public Phrase[] GetAllPhrases()
        {
            
            return null;
        }
        public WordPairDictionary GetPairDictionaryOrNull(int pairId)
        {
            return null;
        }
        public WordPairDictionary[] GetAllPairByWordOrNull(string Word)
        {
            return null;
        }


        // secondary methods
        public string GetSourse(int pairId) => GetPairDictionaryOrNull(pairId).Sourse;

        // private methods
        private bool IsRuWord(string word)
        {
            return true;
        }
    }
}
