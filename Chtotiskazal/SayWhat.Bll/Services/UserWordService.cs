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

public enum CurrentScoreSortingType {
    JustAsked = 1,
    LongAsked = 2
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

        foreach (var word in words) {
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

    public async Task<UserWordModel> GetWordNullByEngWord(UserModel user, string enWord)
        => await _userWordsRepository.GetWordOrDefault(user, enWord);

    public Task AddMutualPhrasesToVocabAsync(UserModel user, int maxCount) {
        return Task.CompletedTask;
        /*
        //var allWords = (await _usersWordsService.GetAllEnWordsForUserAsync(user)).Select(s => s.ToLower().Trim())
        //    .ToHashSet();

        var allWordsForLearning = await _usersWordsService.GetAllUserWords(user);
        var allEngWords = allWordsForLearning.Select(a => a.Word.ToLower().Trim()).ToHashSet();

        var allPhrasesIdForUser = new List<int>();

        foreach (var word in allWordsForLearning)
        {
            var phrases = word.GetPhrasesId();
            allPhrasesIdForUser.AddRange(phrases);
        }


        var allPhrases =  allWordsForLearning.Select(a=>a.Entity) // await _dictionaryService.FindPhrasesBySomeIdsAsync(allPhrasesIdForUser.ToArray());

        var searchedPhrases = new List<Phrase>();
        var endings = 0;
        foreach (var phrase in allPhrases)
        {
            var phraseText = phrase.EnPhrase;
            var count = 0;
            var endingCount = 0;
            foreach (var word in phraseText.Split(new[] {' ', ','}))
            {
                var lowerWord = word.Trim().ToLower();
                if (allEngWords.Contains(lowerWord))
                    count++;
                else if (word.EndsWith('s'))
                {
                    var withoutEnding = lowerWord.Remove(lowerWord.Length - 1);
                    if (allEngWords.Contains(withoutEnding))
                        endingCount++;
                }
                else if (word.EndsWith("ed"))
                {
                    var withoutEnding = lowerWord.Remove(lowerWord.Length - 2);

                    if (allEngWords.Contains(withoutEnding))
                        endingCount++;
                }
                else if (word.EndsWith("ing"))
                {
                    var withoutEnding = lowerWord.Remove(lowerWord.Length - 3);

                    if (allEngWords.Contains(withoutEnding))
                        endingCount++;
                }

                if (count + endingCount <= 1)
                    continue;
                searchedPhrases.Add(phrase);
                if (endingCount > 0)
                {
                    endings++;
                }

                //if (count + endingCount > 2)
                //    Console.WriteLine(phraseText);
            }
        }

        var firstPhrases = searchedPhrases.Randomize().Take(maxCount);
        foreach (var phrase in firstPhrases)
        {
            Console.WriteLine("Adding " + phrase.EnPhrase);
            var userWord =
                UserWordForLearning.CreatePair(phrase.EnPhrase, phrase.PhraseRuTranslate, "[]", isPhrase: true);
            await _usersWordsService.AddWordToUserCollectionAsync(userWord);
        }

        Console.WriteLine($"Found: {searchedPhrases.Count}+{endings}");*/
    }

    public async Task<UserWordModel[]> GetWordsWithPhrasesAsync(
        UserModel user,
        int count,
        int maxTranslations,
        CurrentScoreSortingType sortType,
        double lowRating,
        double? highRating = null) {
        Console.WriteLine($"Рейтинг искомых слов: {lowRating} - {highRating}");
        Console.WriteLine($"Количество мест для слов: {count}");

        Func<FieldDefinition<UserWordModel>, SortDefinition<UserWordModel>> sorting
            = Builders<UserWordModel>.Sort.Ascending;
        if (sortType == CurrentScoreSortingType.JustAsked) {
            sorting = Builders<UserWordModel>.Sort.Descending;
        }

        var wordsForLearning = highRating is null
            ? (await _userWordsRepository.GetWordsForLearningAboveScore(user, count, lowRating)).ToList()
            : (await _userWordsRepository.GetWordsForLearningBetweenLowAndHighScores(user,
                count,
                lowRating,
                highRating.Value,
                sorting))
            .ToList();

        foreach (var wordForLearning in wordsForLearning) {
            var translations = wordForLearning.TextTranslations.ToArray();
            if (translations.Length <= maxTranslations)
                maxTranslations = translations.Length;

            var usedTranslations = translations.Shuffle().Take(maxTranslations).ToArray();
            wordForLearning.RuTranslations = usedTranslations.Select(t => new UserWordTranslation(t)).ToArray();

            // TODO Remove Phrases added as learning words
        }

        Console.WriteLine($"Количество слов: {wordsForLearning.Count()}");
        Console.WriteLine(string.Join(" \r\n", wordsForLearning.ToList()));

        await IncludeExamples(wordsForLearning);
        return wordsForLearning.ToArray();
    }

    public async Task<UserWordModel[]> GetRandomWordsWithPhrasesAsync(
        UserModel user,
        int count,
        int fromNumber,
        int maxTranslations,
        CurrentScoreSortingType sortType,
        double lowRating,
        double highRating) {
        Console.WriteLine($"Рейтинг искомых слов: {lowRating} - {highRating}");
        Console.WriteLine($"Количество мест для слов: {count}");

        Func<FieldDefinition<UserWordModel>, SortDefinition<UserWordModel>> sorting
            = Builders<UserWordModel>.Sort.Ascending;
        if (sortType == CurrentScoreSortingType.JustAsked) {
            sorting = Builders<UserWordModel>.Sort.Descending;
        }

        var wordsForLearning = (await _userWordsRepository.GetWordsForLearningBetweenLowAndHighScores(user,
                fromNumber,
                lowRating,
                highRating,
                sorting))
            .Shuffle()
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