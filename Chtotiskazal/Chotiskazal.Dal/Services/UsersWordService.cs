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
  
        public string[] GetAllUserTranslatesForWord(User user, int id) => _UserWordRepository.GetAllTranslateForUser(user, id);
        
        public int AddWordToUserCollection(User user, int wordId) =>  _UserWordRepository.SaveToUserDictionary(wordId, user.UserId);
        
        //TODO
        public void DeleteWordFromUserCollection(User user, int wordId){}
        
        //TODO
        public UsersPair GetPairByWordOrNull(User user, string word)
        {
            //check ru or eng word?
            //find Pair
            return null;
        }
        
        //TODO
        public UsersPair[] GetPairsForLearning(User user, int count) => _UserWordRepository.GetWorstForUser(user,count);
         
        //TODO
        public UsersPair[] GetPairsForTests(User user, int count, int learnRate) =>
            _UserWordRepository.GetWordsForUserTests(count, learnRate, user);
    }
}
