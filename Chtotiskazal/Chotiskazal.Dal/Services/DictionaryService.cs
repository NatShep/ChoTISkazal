using Chotiskazal.Dal.Repo;
using Chotiskazal.DAL;
using Chotiskazal.LogicR;
using Chotiskazal.LogicR.yapi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chotiskazal.Dal.Enums;

namespace Chotiskazal.Dal.Services
{
    public class DictionaryService
    {
        private readonly DictionaryRepository _dicRepository;

        public DictionaryService( DictionaryRepository repository) => _dicRepository = repository;

        // Add Words or Phrases to dictionary
        public int AddNewWordPairToDictionary(string enword, string ruword, string transcription, TranslationSource sourse)
        {
            var word = new WordDictionary(enword,ruword,transcription,sourse);
            return _dicRepository.AddWordPair(word);
        }
        public int AddPhraseForWordPair(int pairId, string enPhrase, string RuTranslate)
        {
            //look if there are some phrases
            //if not, add prases
            //if there is, update phrases
            return _dicRepository.AddPhrase(pairId, enPhrase,RuTranslate);
        }
        public void AddWordTranslatesAndPhrasesToDictionary(YaDefenition yandexDTO)
        {

        }
        
      
        // Get Translations or Phrases of word
        public string[] GetTranslations(string word)
        {
            return _dicRepository.GetAllTranslate(word);
        }
    //    public Phrase[] GetAllPhrasesByWord(string word) => _dicRepository.GetAllPhrasesForWordOrNull(word);
        public Phrase[] GetAllPhrasesByWordId(int WordPairId)
        {
            return new Phrase[0];
        }
     
        // Get All or single
        public WordDictionary[] GetAllWords() => _dicRepository.GetAllWordPairs();
        public Phrase[] GetAllPhrases()
        {
            
            return null;
        }
        public Phrase[] GetAllPhrasesByWordPair(string word, string t)
        {
            throw new NotImplementedException();
        }
        public WordDictionary GetWordFromDictionaryOrNullById(int pairId)
        {
            return null;
        }
        public WordDictionary[] GetAllTranslateByWordOrNull(string Word)
        {
            return null;
        }


        //Another methods
        public string GetTranscription(string word)
        {
            throw new NotImplementedException();
        }

        // secondary methods
        public TranslationSource GetSourse(int pairId) => GetWordFromDictionaryOrNullById(pairId).Sourse;

        public WordDictionary[] GetWordPairOrNullByWord(string word)
        {
            return _dicRepository.GetWordPairOrNullByWord(word);
        }
    }
}
