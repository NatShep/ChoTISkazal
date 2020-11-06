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
    public class UsersPairsService
    {
        private readonly UserPairsRepo _userPairssRepository;
        
        public UsersPairsService(UserPairsRepo repository) =>_userPairssRepository = repository;

        public int AddWordToUserCollection(int userId, int wordId) =>  
            _userPairssRepository.SaveToUserDictionary(wordId, userId);

        public UserPair GetPairByDicId(int userId, int id) => _userPairssRepository.GetPairByDicIdOrNull(userId, id);

        public UserPair[] GetWorstForUser(int userId, int count) => _userPairssRepository.GetWorstForUser(userId,count);
        
        public UserPair[] GetWorstTestWordForUser(int userId, int count, int learnRate) =>
            _userPairssRepository.GetWorstTestWordsForUser(count, learnRate, userId);
        
        public string[] GetAllUserTranslatesForWord(int userId, string word) => 
            _userPairssRepository.GetAllTranslatesForWordForUser(userId, word);
        
        public string[] GetAllWords(in int userId) =>_userPairssRepository.GetAllWordsForUser(userId);
        
        public Phrase[] GetAllPhrases(int userId) => _userPairssRepository.GetAllPhrases(userId);
        
        //TODO additional methods
        public void DeleteWordFromUserCollection(User user, int wordId){}
        public UserPair[] GetAllLearningWords(in int userId) => throw new NotImplementedException();
        public UserPair[] GetAllPair(int userId) => throw new NotImplementedException();

        


        
    }
}
