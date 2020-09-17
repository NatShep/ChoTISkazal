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
    public class DictionaryService
    {
        private readonly DictionaryRepository _dicRepository;

        public DictionaryService( DictionaryRepository repository) => _dicRepository = repository;

        // Add Words or Phrases to dictionary
        public void AddNewWordToDictionary(string enword, string ruword, string transcription, string sourse)
        {

        }
        public void AddNewWordToDictionary(string enword, string ruword, string transcription, Phrase[] phrases, string sourse)
        {
        }
        public void AddPhrasesForWordPair(int pairId, Phrase[] phrases)
        {
            //look if there are some phrases
            //if not, add prases
            //if there is, update phrases
            _dicRepository.AddPhrases(_dicRepository.GetWordPairOrNull(pairId).Id, phrases);
        }
        public void AddWordTranslatesAndPhrasesToDictionary(YaDefenition yandexDTO)
        {

        }
        
      
        // Get Translations or Phrases of word
        public string[] GetTranslations(string word)
        {
            return _dicRepository.GetAllTranslate(word);
        }
        public Phrase[] GetAllPhrasesByWord(string word) => _dicRepository.GetAllPhrasesForWordOrNull(word);
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
        public string GetSourse(int pairId) => GetWordFromDictionaryOrNullById(pairId).Sourse;
   }
}
