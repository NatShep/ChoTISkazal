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
    public class UsersWordsService
    {
        private readonly UserWordsRepo _userWordsRepository;
        
        public UsersWordsService(UserWordsRepo repository) =>_userWordsRepository = repository;

        public int AddWordToUserCollection(UserWordForLearning userWordForLearning) =>  
            _userWordsRepository.SaveToUserDictionary(userWordForLearning);


        public UserWordForLearning[] GetWorstForUser(int userId, int count) => _userWordsRepository.GetWorstForUser(userId,count);
        
        public UserWordForLearning[] GetWorstTestWordForUser(int userId, int count, int learnRate) =>
            _userWordsRepository.GetWorstTestWordsForUser(count, learnRate, userId);
   
        public string[] GetAllWords(int userId) =>_userWordsRepository.GetAllWordsForUser(userId);
        
        public void RegistrateFailure(UserWordForLearning userWordForLearning)
        {
            userWordForLearning.OnExamFailed();
            userWordForLearning.UpdateAgingAndRandomization();
            _userWordsRepository.UpdateScores(userWordForLearning);
        }
        
        public void UpdateAgingAndRandomize(int count) => _userWordsRepository.UpdateAgingAndRandomization(count);


        public void RegistrateSuccess(UserWordForLearning userWordForLearning)
        {
            userWordForLearning.OnExamPassed();
            userWordForLearning.UpdateAgingAndRandomization();
            _userWordsRepository.UpdateScores(userWordForLearning);
        }        
        //TODO additional methods
        public void DeleteWordFromUserCollection(User user, int wordId){}
        public UserPair[] GetAllLearningWords(in int userId) => throw new NotImplementedException();
        public UserPair[] GetAllPair(int userId) => throw new NotImplementedException();

        


        
    }
}