using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Examples;
using SayWhat.MongoDAL.Users;
using SayWhat.MongoDAL.Words;

namespace SayWhat.Bll.Services;

public enum WordSortingType {
    Descending = 1,
    Ascending = 2
}

public class UsersWordsService {
    private readonly UserWordsRepo _userWordsRepository;
    private readonly ExamplesRepo _examplesRepo;

    public UsersWordsService(UserWordsRepo repository, ExamplesRepo examplesRepo) {
        _userWordsRepository = repository;
        _examplesRepo = examplesRepo;
    }

    public Task AddUserWord(UserWordModel entity) =>
        _userWordsRepository.Add(entity);

    private async Task IncludeExamples(IReadOnlyCollection<UserWordModel> words) {
        var ids = new List<ObjectId>();

        foreach (var word in words) {
            foreach (var translation in word.RuTranslations) {
                ids.AddRange(translation.Examples.Select(e => e.ExampleId));
            }
        }

        var examples = (await _examplesRepo.GetAll(ids)).ToDictionary(e => e.Id);

        foreach (var word in words.Where(w => w.IsWord)) {
            foreach (var translation in word.RuTranslations) {
                foreach (var example in translation.Examples) {
                    example.ExampleOrNull = examples[example.ExampleId];
                }
            }
        }
    }

    public async Task RefreshWordsCurrentScoreAsync(UserModel user) {
        var words = await _userWordsRepository.GetAllWords(user);
        foreach (var word in words) {
            await _userWordsRepository.Update(word);
        }
    }

    public async Task RegisterFailure(UserWordModel userWordModelForLearning, double questionFailScore) {
        userWordModelForLearning.OnQuestionFailed(questionFailScore);
        await _userWordsRepository.UpdateMetrics(userWordModelForLearning);
    }

    public async Task RegisterSuccess(UserWordModel model, double questionPassScore) {
        model.OnQuestionPassed(questionPassScore);
        await _userWordsRepository.UpdateMetrics(model);
    }

    public Task<bool> HasWordsFor(UserModel user) => _userWordsRepository.HasAnyFor(user);

    public Task UpdateWord(UserWordModel model) =>
        _userWordsRepository.Update(model);

    public Task RemoveWord(UserWordModel model) =>
        _userWordsRepository.Remove(model);

    public Task UpdateWordMetrics(UserWordModel model) =>
        _userWordsRepository.UpdateMetrics(model);

    public Task<UserWordModel> GetWordNullByEngWord(UserModel user, string enWord)
        => _userWordsRepository.GetWordOrDefault(user, enWord);
    

    public async Task<UserWordModel[]> GetWordsWithPhrasesAsync(
        UserModel user,
        int count,
        WordSortingType sort,
        double lowRating,
        double? highRating = null,
        int? maxTranslations = null) {
        Func<FieldDefinition<UserWordModel>, SortDefinition<UserWordModel>> sorting =
            sort == WordSortingType.Descending
                ? Builders<UserWordModel>.Sort.Descending
                : Builders<UserWordModel>.Sort.Ascending;

        var words = highRating is null
            ? (await _userWordsRepository.GetWordsAboveScore(user, count, lowRating)).ToList()
            : (await _userWordsRepository.GetWordsBetweenLowAndHighScores(user,
                count,
                lowRating,
                highRating.Value,
                sorting))
            .ToList();

        foreach (var wordForLearning in words) {
            string[] usedTranslations;
            if (maxTranslations != null) {
                var translations = wordForLearning.TextTranslations.ToArray();
                var maxTranslationForWord = maxTranslations.Value;
                if (translations.Length <= maxTranslationForWord)
                    maxTranslationForWord = translations.Length;
                usedTranslations = translations.Take(maxTranslationForWord).ToArray();
            }
            else
                usedTranslations = wordForLearning.TextTranslations.ToArray();

            wordForLearning.RuTranslations = usedTranslations.Select(t => new UserWordTranslation(t)).ToArray();
        }

        await IncludeExamples(words);
        return words.ToArray();
    }

    public async Task<UserWordModel[]> GetRandomWordsWithPhrasesAsync(
        UserModel user,
        int count,
        int fromNumber,
        int maxTranslations,
        WordSortingType sortType,
        double lowRating,
        double highRating) {
        Console.WriteLine($"Рейтинг искомых слов: {lowRating} - {highRating}");
        Console.WriteLine($"Количество мест для слов: {count}");

        Func<FieldDefinition<UserWordModel>, SortDefinition<UserWordModel>> sorting
            = Builders<UserWordModel>.Sort.Ascending;
        if (sortType == WordSortingType.Descending) {
            sorting = Builders<UserWordModel>.Sort.Descending;
        }

        var wordsForLearning = (await _userWordsRepository.GetWordsBetweenLowAndHighScores(user,
                fromNumber,
                lowRating,
                highRating,
                sorting))
            .Shuffle()
            .Where(u => u.IsWord)
            .Take(count)
            .ToList();

        foreach (var wordForLearning in wordsForLearning) {
            var translations = wordForLearning.RuTranslations.ToArray();
            if (translations.Length <= maxTranslations)
                maxTranslations = translations.Length;

            var usedTranslations = translations.Shuffle().Take(maxTranslations).ToArray();
            wordForLearning.RuTranslations = usedTranslations;

            // TODO Remove Phrases added as learning words
        }

        Console.WriteLine($"Количество слов: {wordsForLearning.Count()}");
        Console.WriteLine(string.Join(" \r\n", wordsForLearning.ToList()));

        await IncludeExamples(wordsForLearning);
        return wordsForLearning.ToArray();
    }

    public Task<IReadOnlyCollection<UserWordModel>> GetAllWords(UserModel user)
        => _userWordsRepository.GetAllWords(user);

    public Task<bool> Contains(UserModel user, string word) => _userWordsRepository.Contains(user, word);
}