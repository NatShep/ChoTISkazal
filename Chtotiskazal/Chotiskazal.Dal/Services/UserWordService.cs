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
    public class UsersWordsService
    {
        private readonly UserWordsRepo _userWordsRepository;
        
        public UsersWordsService(UserWordsRepo repository) =>_userWordsRepository = repository;

        public async Task<int> AddWordToUserCollectionAsync(UserWordForLearning userWordForLearning) =>  
           await  _userWordsRepository.SaveToUserDictionaryAsync(userWordForLearning);


        public async Task<UserWordForLearning[]> GetWorstForUserWithPhrasesAsync(int userId, int count) => 
            await _userWordsRepository.GetWorstForUserWithPhrasesAsync(userId,count);
        
        public async Task<UserWordForLearning[]> GetWorstTestWordForUserAsync(int userId, int count, int learnRate) =>
            await _userWordsRepository.GetWorstTestWordsForUserAsync(count, learnRate, userId);
   
        public async Task<string[]> GetAllWordsAsync(int userId) =>await _userWordsRepository.GetAllWordsForUserAsync(userId);
        
        public async Task<UserWordForLearning[]> GetAllUserWordsForLearningAsync(int userId) =>
            await _userWordsRepository.GetAllUserWordsForLearningAsync(userId);

        public async Task RegistrateFailureAsync(UserWordForLearning userWordForLearning)
        {
            userWordForLearning.OnExamFailed();
            userWordForLearning.UpdateAgingAndRandomization();
            await _userWordsRepository.UpdateScoresAsync(userWordForLearning);
        }
        
        public async Task UpdateAgingAndRandomizeAsync(int count) => await _userWordsRepository.UpdateAgingAndRandomizationAsync(count);


        public async Task RegistrateSuccessAwait(UserWordForLearning userWordForLearning)
        {
            userWordForLearning.OnExamPassed();
            userWordForLearning.UpdateAgingAndRandomization();
            await _userWordsRepository.UpdateScoresAsync(userWordForLearning);
        }

        public async Task AddPhraseAsWordToUserCollectionAsync(Phrase phrase)
        {
            var userWord=new UserWordForLearning()
            {
                    EnWord=phrase.EnPhrase,
                    UserTranslations = phrase.PhraseRuTranslate,
                    Transcription = "",
                    PhrasesIds = "",
                    IsPhrase = true,
            };
            await _userWordsRepository.SaveToUserDictionaryAsync(userWord);
        }

        public async Task<UserWordForLearning> GetWordForLearningOrNullByWordAsync(int userId, string enWord) =>
            await _userWordsRepository.GetWordByEnWordOrNullAsync(userId, enWord);



        //TODO additional methods
        public void DeleteWordFromUserCollection(User user, int wordId){}
        public UserPair[] GetAllLearningWords(in int userId) => throw new NotImplementedException();


        public void UpdateWord(UserWordForLearning userWord) =>
            _userWordsRepository.UpdateWordTranslations(userWord);
    }
}