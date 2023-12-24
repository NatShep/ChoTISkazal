using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Examples;
using SayWhat.MongoDAL.Users;
using SayWhat.MongoDAL.Words;
using Serilog;

namespace SayWhat.Bll.Services;

public class MutualPhrasesService {
    private readonly ExamplesRepo _examplesRepo;
    private readonly UsersWordsService _usersWordsService;
    private readonly ILogger _logger;

    public MutualPhrasesService(ExamplesRepo examplesRepo, UsersWordsService usersWordsService, ILogger logger) {
        _examplesRepo = examplesRepo;
        _usersWordsService = usersWordsService;
        _logger = logger;
    }

    public Task<List<Example>> GetAllExamples() => _examplesRepo.GetAll();

    public async Task<int> AddMutualPhrasesToUser(UserModel user, IList<MutualPhrase> phrases) {
        _logger.Debug($"AddMutualPhrasesToUser {user.TelegramNick}");
        var resultPhraseCount = 0;
        var allUserWords = await _usersWordsService.GetAllWords(user);

        foreach (var foundPhrase in phrases) {
            if (allUserWords.Any(a =>
                    string.Equals(a.Word, foundPhrase.Origin.OriginPhrase,
                        StringComparison.InvariantCultureIgnoreCase)))
                continue;
            var word = new UserWordModel(
                userId: user.Id,
                word: foundPhrase.Origin.OriginPhrase,
                direction: TranslationDirection.EnRu,
                absScore: 0,
                source: TranslationSource.AutoPhrase,
                wordType: UserWordType.Phrase,
                translation: new UserWordTranslation
                {
                    Transcription = "",
                    Word = foundPhrase.Origin.TranslatedPhrase,
                    Examples = Array.Empty<UserWordTranslationReferenceToExample>(),
                });
            _logger.Debug("        Add phrase " + word.Word + " -> " + word.RuTranslations[0].Word);
            await _usersWordsService.AddUserWord(word);
            resultPhraseCount++;
        }

        return resultPhraseCount;
    }

    public async Task<IList<MutualPhrase>> FindMutualPhrases(UserModel user, IList<Example> allExamples) {
        _logger.Debug($"FindMutualPhrases to user {user.TelegramNick}");
        int resultPhraseCount = 0;
        var allUserWords = await _usersWordsService.GetAllWords(user);
        var words = allUserWords
            .Where(u => u.IsWord && u.AbsoluteScore > 3 && u.HasAnyExamples)
            .ToArray();

        var allEngWords = words.Where(w => w.IsWord)
            .ToDictionary(w => w.Word.ToLower(), w => w.RuTranslations.Select(r => r.Word).ToArray());

        var foundPhrases = new List<MutualPhrase>();
        var endings = 0;
        HashSet<string> checkedExamples = new HashSet<string>();
        foreach (var example in allExamples) {
            if (!checkedExamples.Add(example.OriginPhrase))
                continue;

            var phraseText = example.OriginPhrase;
            var totalCount = 0;
            var totalEndingCount = 0;

            foreach (var enWord in phraseText.Split(' ', ',', '`')) {
                if (!DoesPhraseContainsEnglishWords(enWord, out var count, out var endingCount, out var ruTranslations))
                    continue;
                // if (!DoesExampleContainsSomeOfRuWords(example.TranslatedPhrase, ruTranslations))
                //     continue;
                totalCount += count;
                totalEndingCount += endingCount;
            }

            if (totalCount + totalEndingCount < 2)
                continue;
            foundPhrases.Add(new MutualPhrase(example, totalCount, totalEndingCount));
        }

        _logger.Debug($"FindMutualPhrases to user {user.TelegramNick} returns {foundPhrases.Count} phrases");

        return foundPhrases;

        bool DoesExampleContainsSomeOfRuWords(string ruPhrase, string[] ruWords) {
            var wordsInPhrase = ruPhrase.Split(' ', ',', '`').Select(NormalizeRuWord).ToArray();
            var actualWords = ruWords.Select(NormalizeRuWord).ToArray();
            foreach (var wordInPhrase in wordsInPhrase) {
                foreach (var actualWord in actualWords) {
                    var distance = Fastenshtein.Levenshtein.Distance(wordInPhrase, actualWord);
                    if (distance == 0)
                        return true;
                    //small mistakes: one mistake for each 4 letters
                    //big   mistakes: one mistake for each 3 letters
                    int length = Math.Min(wordInPhrase.Length, actualWord.Length);
                    if (distance <= length / 3)
                        return true;
                }
            }

            return false;
        }

        bool DoesPhraseContainsEnglishWords(string enWord, out int count, out int endingCount,
            out string[] ruTranslations) {
            var lowerWord = enWord.Trim().ToLower();
            count = 0;
            endingCount = 0;
            if (allEngWords.TryGetValue(lowerWord, out ruTranslations))
                count++;
            else {
                var normalized = NormalizeEnWord(enWord);
                if (normalized.Length != enWord.Length) {
                    if (allEngWords.TryGetValue(normalized, out ruTranslations))
                        endingCount++;
                }
            }

            return count != 0 || endingCount != 0;
        }
    }

    static readonly string[] RuEndings =
    {
        "а", "о", "я", "е", "ы", "е", "у", "ю",
        "ая", "яя", "ое", "ее", "ой", "ые", "ие", "ый", "йй", "ей", "", "", "", "",
        "ать", "ять", "оть", "еть", "уть", "ем", "им", "ешь", "ишь", "ете", "ите", "ём", "ёте", "ет", "ит", "ут",
        "ют", "ят", "ал", "ял", "ала", "яла", "али", "яли", "ол", "ел", "ола", "ела", "оли", "ели", "ул", "ула",
        "ули", "ого", "его", "ых", "их", "ым", "им", "ому", "ему", "ую", "юю", "ий", "ом", "шим", "ший", "щим", "щий"
    };

    private static readonly string[] EnEndings =
    {
        "s", "ed", "ing"
    };

    string RemoveEnding(string word, params string[] endings) {
        if (word.Length == 0)
            return word;
        var endingsByLength = endings.GroupBy(e => e.Length).OrderByDescending(e => e.Key);
        foreach (var endingGroup in endingsByLength) {
            if (word.Length <= endingGroup.Key)
                continue;
            foreach (var ending in endingGroup) {
                if (word.EndsWith(ending))
                    return word[..^ending.Length];
            }
        }

        return word;
    }

    string NormalizeEnWord(string word) => RemoveEnding(word, EnEndings);

    string NormalizeRuWord(string word) => RemoveEnding(word, RuEndings);
}

public record MutualPhrase(Example Origin, int wordsCount, int endingCount) {
    public override string ToString() => Origin.OriginPhrase + " -> " + Origin.TranslatedPhrase;
}