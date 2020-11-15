using Chotiskazal.Dal.Repo;
using Chotiskazal.DAL;
using Chotiskazal.LogicR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Phrase = Chotiskazal.DAL.Phrase;

namespace Chotiskazal.Dal.Services
{
    public class UsersPairsService
    {
        private readonly UserPairsRepo _userPairssRepository;
        
        public UsersPairsService(UserPairsRepo repository) =>_userPairssRepository = repository;

        public async Task<int> AddWordToUserCollectionAsync(int userId, int wordId) =>  
           await _userPairssRepository.SaveToUserDictionaryAsync(wordId, userId);

        public async Task<UserPair> GetPairByDicIdAsync(int userId, int id) => await _userPairssRepository.GetPairByDicIdOrNullAsync(userId, id);

        public async Task<UserPair[]> GetWorstForUserAsync(int userId, int count) => 
            await _userPairssRepository.GetWorstForUserAsync(userId,count);
        
        public async Task<UserPair[]> GetWorstTestWordForUserAsync(int userId, int count, int learnRate) =>
          await  _userPairssRepository.GetWorstTestWordsForUserAsync(count, learnRate, userId);
        
        public async Task<string[]> GetAllUserTranslatesForWordAsync(int userId, string word) => 
            await _userPairssRepository.GetAllTranslatesForWordForUserAsync(userId, word);
        
        public async Task<string[]> GetAllWordsAsync(int userId) =>await _userPairssRepository.GetAllWordsForUserAsync(userId);
        
        public async Task<Phrase[]> GetAllPhrasesAsync(int userId) =>await _userPairssRepository.GetAllPhrasesAsync(userId);
        
        //TODO additional methods
        public void DeleteWordFromUserCollection(User user, int wordId){}
        public UserPair[] GetAllLearningWords(in int userId) => throw new NotImplementedException();
        public UserPair[] GetAllPair(int userId) => throw new NotImplementedException();

        


        
    }
}
