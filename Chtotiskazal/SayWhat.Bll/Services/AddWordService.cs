using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using SayWhat.Bll.Strings;
using SayWhat.Bll.Yapi;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Dictionary;
using SayWhat.MongoDAL.Examples;
using SayWhat.MongoDAL.Users;
using SayWhat.MongoDAL.Words;
using Language = SayWhat.MongoDAL.Language;
using Translation = SayWhat.Bll.Dto.Translation;

namespace SayWhat.Bll.Services {

public class AddWordService {
    private readonly UsersWordsService _usersWordsService;
    private readonly YandexDictionaryApiClient _yaDicClient;
    private readonly LocalDictionaryService _localDictionaryService;
    private readonly UserService _userService;

    public AddWordService(
        UsersWordsService usersWordsService,
        YandexDictionaryApiClient yaDicClient,
        LocalDictionaryService localDictionaryService,
        UserService userService) {
        _usersWordsService = usersWordsService;
        _yaDicClient = yaDicClient;
        _localDictionaryService = localDictionaryService;
        _userService = userService;
    }

    public async Task<IReadOnlyList<Translation>> TranslateAndAddToDictionary(string originWord) {
        originWord = originWord.ToLower();

        if (originWord.Count(e => e == ' ') >= 3) return null;
        //todo go to translate api
        
        IReadOnlyList<Translation> res = null;
        return originWord.IsRussian()
            ? await TranslateRuEnWordAndAddItToDictionary(originWord)
            : await TranslateEnRuWordAndAddItToDictionary(originWord);
    }

    public async Task<IReadOnlyList<Translation>> GetOrDownloadTranslation(string enWord) {
        if (enWord.IsRussian())
            throw new ArgumentException("Only en words allowed");
        var translations = await FindInLocalDictionaryWithExamples(enWord);
        if (!translations.Any()) // if not, search it in Ya dictionary
            translations = await TranslateAndAddToDictionary(enWord);

        // Inline keyboards has limitation for size of the message 
        // Workaraound: exclude all translations that are more than 32 symbols
        if (translations != null && translations.Any(t => t.OriginText.Length + t.TranslatedText.Length > 32))
            translations = translations.Where(t => t.OriginText.Length + t.TranslatedText.Length <= 32).ToArray();
        return translations;
    }

    private async Task<IReadOnlyList<Translation>> TranslateEnRuWordAndAddItToDictionary(string englishWord) {
        // Go to yandex api
        var yaResponse = await _yaDicClient.EnRuTranslateAsync(englishWord);
        if (yaResponse?.Any() != true)
            return Array.Empty<Translation>();
        var word = ChaosBllHelper.ConvertToDictionaryWord(englishWord, yaResponse, Language.En, Language.Ru);
        try
        {
            await _localDictionaryService.AddNewWord(word);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        return word.ToDictionaryTranslations();
    }

    private async Task<IReadOnlyList<Translation>> TranslateRuEnWordAndAddItToDictionary(string russianWord) {
        // Go to yandex api
        var yaResponse = await _yaDicClient.RuEnTranslateAsync(russianWord);
        if (yaResponse?.Any() != true)
            return Array.Empty<Translation>();
        var word = ChaosBllHelper.ConvertToDictionaryWord(russianWord, yaResponse, Language.Ru, Language.En);
        await _localDictionaryService.AddNewWord(word);
        return word.ToDictionaryTranslations();
    }
    
    public async Task<IReadOnlyList<Translation>> FindInLocalDictionaryWithExamples(string word)
        => await _localDictionaryService.GetTranslationsWithExamplesByEnWord(word.ToLower());

    public async Task AddTranslationToUser(UserModel user, Translation translation) {
        if (translation == null) return;
        if (translation.TranslationDirection != TranslationDirection.EnRu)
            throw new InvalidOperationException("Only en-ru direction is supported");

        var alreadyExistsWord = await _usersWordsService.GetWordNullByEngWord(user, translation.OriginText);

        if (alreadyExistsWord == null)
        {
            //the Word is new for the user
            var word = new UserWordModel(
                userId: user.Id,
                word: translation.OriginText,
                direction: TranslationDirection.EnRu,
                absScore: 0,
                translation: new UserWordTranslation {
                    Transcription = translation.EnTranscription,
                    Word = translation.TranslatedText,
                    Examples = translation.Examples
                                          .Select(p => new UserWordTranslationReferenceToExample(p.Id))
                                          .ToArray()
                });
            word.UpdateCurrentScore();
            await _usersWordsService.AddUserWord(word);

            user.OnNewWordAdded(
                statsChanging: WordStatsChanging.CreateForNewWord(word.AbsoluteScore),
                pairsCount: word.RuTranslations.Length,
                examplesCount: word.RuTranslations.Sum(t => t.Examples?.Length ?? 0));
        }
        else
        {
            // User already have the word.
            var originRate = alreadyExistsWord.Score;
            var translates = alreadyExistsWord.TextTranslations.ToList();
            var newTranslations = new List<UserWordTranslation>();
            var r = translation;
            if (!translates.Contains(r.TranslatedText))
                newTranslations.Add(
                    new UserWordTranslation {
                        Transcription = r.EnTranscription,
                        Word = r.TranslatedText,
                        Examples = r.Examples.Select(p => new UserWordTranslationReferenceToExample(p.Id)).ToArray()
                    });

            alreadyExistsWord.OnQuestionFailed();

            if (newTranslations.Count == 0)
            {
                await _usersWordsService.UpdateWordMetrics(alreadyExistsWord);
                return;
            }

            alreadyExistsWord.AddTranslations(newTranslations);
            await _usersWordsService.UpdateWord(alreadyExistsWord);
            user.OnPairsAdded(
                statsChanging: alreadyExistsWord.Score - originRate,
                newTranslations.Count,
                newTranslations.Sum(t => t.Examples?.Length ?? 0));
        }

        await _userService.Update(user);
    }

    public async Task RemoveTranslationFromUser(UserModel user, Translation translation) {
        if (translation == null) return;
        if (translation.TranslationDirection != TranslationDirection.EnRu)
            throw new InvalidOperationException("Only en-ru direction is supported");

        var alreadyExistsWord = await _usersWordsService.GetWordNullByEngWord(user, translation.OriginText);
        if (alreadyExistsWord == null)
            return;
        var scoreBefore = alreadyExistsWord.Score;
        var tr = alreadyExistsWord.RemoveTranslation(translation.TranslatedText);
        if (tr == null)
            return;

        user.OnPairRemoved(tr, alreadyExistsWord.Score - scoreBefore);
        if (alreadyExistsWord.RuTranslations.Length == 0)
        {
            await _usersWordsService.RemoveWord(alreadyExistsWord);
            user.OnWordRemoved(alreadyExistsWord);
        }
        else
            await _usersWordsService.UpdateWord(alreadyExistsWord);
    }

    public Task<UserWordModel> GetWordNullByEngWord(UserModel chatUser, string word)
        => _usersWordsService.GetWordNullByEngWord(chatUser, word);
}

}