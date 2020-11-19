using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SayWhat.MongoDAL.Users;
using SayWhat.MongoDAL.Words;

namespace SayWhat.Bll.Services
{
    public class UsersWordsService
    {
        private readonly UserWordsRepo _userWordsRepository;

        public UsersWordsService(UserWordsRepo repository) => _userWordsRepository = repository;

        public Task AddWordToUserCollectionAsync(User user, UserWordModel model) =>
             _userWordsRepository.Add(model.Entity);

        public async Task<IEnumerable<UserWordModel>> GetWorstForUserWithPhrasesAsync(User user, int count)
        {
            var words =await _userWordsRepository.GetWorstLearned(user, count);
            return words.Select(w => new UserWordModel(w));
        }

        public async Task<UserWordModel[]> GetWorstWordsForUserAsync(User user, int maxCount, int learnRate) =>
            (await _userWordsRepository.GetWorstLearned(user,  maxCount, learnRate))
            .Select(t=> new UserWordModel(t))
            .ToArray();

        public async Task<string[]> GetAllEnWordsForUserAsync(User user) =>
            (await _userWordsRepository.GetAllUserWordsAsync(user))
            .Select(w=>w.Word)
            .ToArray();

        public async Task<UserWordModel[]> GetAllUserWords(User user) =>
            (await _userWordsRepository.GetAllUserWordsAsync(user))
            .Select(w=> new UserWordModel(w))
            .ToArray();

        public async Task RegisterFailureAsync(UserWordModel userWordForLearning)
        {
            userWordForLearning.OnExamFailed();
            userWordForLearning.UpdateAgingAndRandomization();
            await _userWordsRepository.UpdateScores(userWordForLearning.Entity);
        }

        public async Task UpdateAgingAndRandomizeAsync(User user, int count)
        {
            var words = await _userWordsRepository.GetRandowWords(user, count);
            foreach (var word in words)
            {
                var model = new UserWordModel(word);
                model.UpdateAgingAndRandomization();
                await _userWordsRepository.Update(model.Entity);
            }
        }

        public async Task RegisterSuccessAwait(UserWordModel model)
        {
            model.OnExamPassed();
            model.UpdateAgingAndRandomization();
            await _userWordsRepository.UpdateScores(model.Entity);
        }

        public Task<bool> HasWords(User user) => _userWordsRepository.HasAnyFor(user);
        public Task UpdateWord(UserWordModel model) =>
             _userWordsRepository.Update(model.Entity);

        public async Task<UserWordModel> GetWordNullByEngWord(User user, string enWord)
        {
            var word = await _userWordsRepository.GetWordOrDefault(user, enWord);
            if (word == null)
                return null;
            return new UserWordModel(word);
        }
    }
}