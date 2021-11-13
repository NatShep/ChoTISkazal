using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using SayWhat.Bll.Yapi;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Dictionary;
using SayWhat.MongoDAL.Examples;
using SayWhat.MongoDAL.Users;
using SayWhat.MongoDAL.Words;
using DictionaryTranslation = SayWhat.Bll.Dto.DictionaryTranslation;
using Language = SayWhat.MongoDAL.Language;

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

    public async Task<IReadOnlyList<DictionaryTranslation>> TranslateAndAddToDictionary(string originWord) {
        originWord = originWord.ToLower();

        if (originWord.Count(e => e == ' ') < 4)
        {
            IReadOnlyList<DictionaryTranslation> res = null;
            if (originWord.IsRussian())
                res = await TranslateRuEnWordAndAddItToDictionary(originWord);
            else
                res = await TranslateEnRuWordAndAddItToDictionary(originWord);

            if (res != null)
                return res;
        }

        //todo go to translate api
        return null;
    }

    public async Task<IReadOnlyList<DictionaryTranslation>> GetOrDownloadTranslation(string enWord) {
        if (enWord.IsRussian())
            throw new ArgumentException("Only en words allowed");
        var translations = await FindInDictionaryWithExamples(enWord);
        if (!translations.Any()) // if not, search it in Ya dictionary
            translations = await TranslateAndAddToDictionary(enWord);

        // Inline keyboards has limitation for size of the message 
        // Workaraound: exclude all translations that are more than 32 symbols
        if (translations != null && translations.Any(t => t.OriginText.Length + t.TranslatedText.Length > 32))
            translations = translations.Where(t => t.OriginText.Length + t.TranslatedText.Length <= 32).ToArray();
        return translations;
    }

    private async Task<IReadOnlyList<DictionaryTranslation>> TranslateEnRuWordAndAddItToDictionary(string englishWord) {
        // Go to yandex api
        var yaResponse = await _yaDicClient.EnRuTranslateAsync(englishWord);
        if (yaResponse?.Any() != true)
            return Array.Empty<DictionaryTranslation>();
        var word = ConvertToDictionaryWord(englishWord, yaResponse, Language.En, Language.Ru);
        try
        {
            await _localDictionaryService.AddNewWord(word);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        return ToDictionaryTranslations(word);
    }

    private async Task<IReadOnlyList<DictionaryTranslation>> TranslateRuEnWordAndAddItToDictionary(string russianWord) {
        // Go to yandex api
        var yaResponse = await _yaDicClient.RuEnTranslateAsync(russianWord);
        if (yaResponse?.Any() != true)
            return new DictionaryTranslation[0];
        var word = ConvertToDictionaryWord(russianWord, yaResponse, Language.Ru, Language.En);
        await _localDictionaryService.AddNewWord(word);
        return ToDictionaryTranslations(word);
    }

    private static IReadOnlyList<DictionaryTranslation> ToDictionaryTranslations(DictionaryWord word) {
        return word.Translations.Select(
                       t => new DictionaryTranslation(
                           originText: word.Word,
                           translatedText: t.Word,
                           originTranscription: word.Transcription,
                           source: word.Source,
                           tranlationDirection: word.Language == Language.En
                               ? TranslationDirection.EnRu
                               : TranslationDirection.RuEn,
                           phrases: t.Examples.Select(e => e.ExampleOrNull).ToList()
                       ))
                   .ToArray();
    }

    private static DictionaryWord ConvertToDictionaryWord(
        string originText, YaDefenition[] definitions,
        Language langFrom,
        Language langTo
    ) {
        if (langFrom == langTo)
            throw new InvalidOperationException();

        var variants = definitions.SelectMany(
            r => r.Tr.Select(
                tr => new {
                    defenition = r,
                    translation = tr,
                }));

        var word = new DictionaryWord {
            Id = ObjectId.GenerateNewId(),
            Language = langFrom,
            Word = originText,
            Source = TranslationSource.Yadic,
            Transcription = definitions.FirstOrDefault()?.Ts,
            Translations = variants.Select(
                                       v => new SayWhat.MongoDAL.Dictionary.DictionaryTranslation {
                                           Word = v.translation.Text,
                                           Language = langTo,
                                           Examples = v.translation.Ex?.Select(
                                                           e =>
                                                               new DictionaryReferenceToExample(
                                                                   new Example {
                                                                       Id = ObjectId.GenerateNewId(),
                                                                       OriginWord = originText,
                                                                       TranslatedWord = v.translation.Text,
                                                                       Direction = langFrom == Language.En
                                                                           ? TranslationDirection.EnRu
                                                                           : TranslationDirection.RuEn,
                                                                       OriginPhrase = e.Text,
                                                                       TranslatedPhrase = e.Tr.First().Text,
                                                                   }))
                                                       .ToArray() ??
                                                      Array.Empty<DictionaryReferenceToExample>()
                                       })
                                   .ToArray()
        };
        return word;
    }

    public async Task<IReadOnlyList<DictionaryTranslation>> FindInDictionaryWithExamples(string word)
        => await _localDictionaryService.GetTranslationsWithExamples(word.ToLower());

    public async Task AddTranslationToUser(UserModel user, DictionaryTranslation translation) {
        if (translation == null) return;
        if (translation.TranlationDirection != TranslationDirection.EnRu)
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
                pairsCount: word.Translations.Length,
                examplesCount: word.Translations.Sum(t => t.Examples?.Length ?? 0));
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

    public async Task RemoveTranslationFromUser(UserModel user, DictionaryTranslation translation) {
        if (translation == null) return;
        if (translation.TranlationDirection != TranslationDirection.EnRu)
            throw new InvalidOperationException("Only en-ru direction is supported");

        var alreadyExistsWord = await _usersWordsService.GetWordNullByEngWord(user, translation.OriginText);
        if (alreadyExistsWord == null)
            return;
        var scoreBefore = alreadyExistsWord.Score;
        var tr = alreadyExistsWord.RemoveTranslation(translation.TranslatedText);
        if (tr == null)
            return;

        user.OnPairRemoved(tr, alreadyExistsWord.Score - scoreBefore);
        if (alreadyExistsWord.Translations.Length == 0)
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