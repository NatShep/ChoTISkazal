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
  
        public string[] GetAllUserTranslatesForWord(User user, string word) => _UserWordRepository.GetAllTranslateForWord(user, word);
        
        public int AddWordToUserCollection(User user, int wordId) =>  _UserWordRepository.SaveToUserDictionary(wordId, user.UserId);

        public UserPair GetPairByDicId(User user, int id) => _UserWordRepository.GetPairByDicIdOrNull(user, id);
            
        //TODO
        public void DeleteWordFromUserCollection(User user, int wordId){}
        
        //TODO
        public UserPair[] GetPairsByWordOrNull(User user, string word)
        {
            //check ru or eng word?
            //find Pair
            return null;
        }
        
        //TODO
        public UserPair[] GetPairsForLearning(User user, int count) => _UserWordRepository.GetWorstForUser(user,count);
         
        //TODO
        public UserPair[] GetPairsForTests(User user, int count, int learnRate) =>
            _UserWordRepository.GetWordsForUserTests(count, learnRate, user);

      
    }
}
