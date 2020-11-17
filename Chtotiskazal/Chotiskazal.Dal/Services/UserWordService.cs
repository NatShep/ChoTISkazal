using Chotiskazal.Dal.Repo;
using System.Threading.Tasks;
using Chotiskazal.Dal.DAL;

namespace Chotiskazal.Dal.Services
{
    public class UsersWordsService
    {
        private readonly UserWordsRepo _userWordsRepository;

        public UsersWordsService(UserWordsRepo repository) => _userWordsRepository = repository;

        public async Task<int> AddWordToUserCollectionAsync(UserWordForLearning userWordForLearning) =>
            await _userWordsRepository.SaveToUserDictionaryAsync(userWordForLearning);
      
        public async Task<UserWordForLearning[]> GetWorstForUserWithPhrasesAsync(int userId, int count) =>
            await _userWordsRepository.GetWorstForUserWithPhrasesAsync(userId, count);

        public async Task<UserWordForLearning[]> GetWorstWordsForUserAsync(int userId, int count, int learnRate) =>
            await _userWordsRepository.GetWorstTestWordsWithPhrasesForUserAsync(count, learnRate, userId);

        public async Task<string[]> GetAllEnWordsForUserAsync(int userId) =>
            await _userWordsRepository.GetAllEnWordsForUserAsync(userId);

        public async Task<UserWordForLearning[]> GetAllUserWordsWithPhrasesForUserAsync(int userId) =>
            await _userWordsRepository.GetAllUserWordsAsync(userId);

        public async Task RegisterFailureAsync(UserWordForLearning userWordForLearning)
        {
            userWordForLearning.OnExamFailed();
            userWordForLearning.UpdateAgingAndRandomization();
            await _userWordsRepository.UpdateScoresAsync(userWordForLearning);
        }

        public async Task UpdateAgingAndRandomizeAsync(int userId, int count) =>
            await _userWordsRepository.UpdateAgingAndRandomizationAsync(userId, count);


        public async Task RegisterSuccessAwait(UserWordForLearning userWordForLearning)
        {
            userWordForLearning.OnExamPassed();
            userWordForLearning.UpdateAgingAndRandomization();
            await _userWordsRepository.UpdateScoresAsync(userWordForLearning);
        }


        public async Task<UserWordForLearning> GetWordForLearningOrNullByEnWordAsync(int userId, string enWord) =>
            await _userWordsRepository.GetWordForLearningByEnWordOrNullAsync(userId, enWord);


        public async Task<bool> GetAnyWordAsync(int userId)
        {
            var word = await _userWordsRepository.GetAnyWordAsync(userId);
            return word != null;
        }

        public void UpdateWord(UserWordForLearning userWord) =>
            _userWordsRepository.UpdateWordTranslations(userWord);
    }
}