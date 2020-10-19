using Chotiskazal.Dal.Repo;
using Chotiskazal.DAL;
using Chotiskazal.LogicR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phrase = Chotiskazal.DAL.Phrase;

namespace Chotiskazal.Dal.Services
{
    public class UsersWordService
    {
        private readonly UserWordsRepo _UserWordRepository;
        
        public UsersWordService(UserWordsRepo repository) =>_UserWordRepository = repository;

        public int AddWordToUserCollection(int userId, int wordId) =>  
            _UserWordRepository.SaveToUserDictionary(wordId, userId);

        public UserPair GetPairByDicId(int userId, int id) => _UserWordRepository.GetPairByDicIdOrNull(userId, id);

        public UserPair[] GetWorstForUser(int userId, int count) => _UserWordRepository.GetWorstForUser(userId,count);
        
        public UserPair[] GetWorstTestWordForUser(int userId, int count, int learnRate) =>
            _UserWordRepository.GetWorstTestWordsForUser(count, learnRate, userId);
        
        public string[] GetAllUserTranslatesForWord(int userId, string word) => 
            _UserWordRepository.GetAllTranslatesForWordForUser(userId, word);
        
        public string[] GetAllWords(in int userId) =>_UserWordRepository.GetAllWordsForUser(userId);
        
        public Phrase[] GetAllPhrases(int userId) => _UserWordRepository.GetAllPhrases(userId);
        
        //TODO additional methods
        public void DeleteWordFromUserCollection(User user, int wordId){}
        public UserPair[] GetAllLearningWords(in int userId) => throw new NotImplementedException();
        public UserPair[] GetAllPair(int userId) => throw new NotImplementedException();

        


        
    }
}
