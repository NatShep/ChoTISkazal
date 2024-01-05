using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.FrequentWords;
using SayWhat.MongoDAL.Users;
using SayWhat.MongoDAL.Words;

namespace SayWhat.Bll.Services;

/// <summary>
/// Операции для администрирования бота. Вызываются из вручную написанного кода
/// </summary>
public class AdminToolsService {
    private readonly UsersRepo _userRepo;
    private readonly UserWordsRepo _userWordRepo;
    private readonly LocalDictionaryService _localDictionaryService;
    private readonly AddWordService _addWordService;
    private readonly LearningSetService _learningSetService;
    private readonly FrequentWordsRepo _frequentWordsRepo;

    public AdminToolsService(UsersRepo userRepo, UserWordsRepo userWordRepo,
        LocalDictionaryService localDictionaryService, 
        AddWordService addWordService, 
        LearningSetService learningSetService, FrequentWordsRepo frequentWordsRepo) {
        _userRepo = userRepo;
        _userWordRepo = userWordRepo;
        _localDictionaryService = localDictionaryService;
        _addWordService = addWordService;
        _learningSetService = learningSetService;
        _frequentWordsRepo = frequentWordsRepo;
    }

    public async Task ReportForNotSynchronizedUserWordsAndLocalDictionary() {
        var allUsers = _userRepo.GetAll();

        foreach (var user in allUsers) {
            var allWords = await _userWordRepo.GetAllWords(user);
            foreach (var word in allWords) {
                var translations = await _localDictionaryService.GetAllTranslationWords(word.Word);
                if (!translations.Any()) {
                    Console.WriteLine($"No translations for {user.TelegramNick}:{word.Word}");
                }
                else {
                    var hasSimilar = word.RuTranslations.Any(
                        t => translations.Any(o => o.Equals(t.Word, StringComparison.InvariantCultureIgnoreCase)));
                    if (hasSimilar) continue;

                    Console.WriteLine($"Misstranslation for {user.TelegramNick}:{word.Word}");
                }
            }
        }
    }

    public async Task SyncronizeUserWordsAndLocalDictionary() {
        var allUsers = _userRepo.GetAll();

        foreach (var user in allUsers) {
            var allWords = await _userWordRepo.GetAllWords(user);
            foreach (var word in allWords) {
                var translations = await _localDictionaryService.GetAllTranslationWords(word.Word);
                if (!translations.Any()) {
                    Console.WriteLine($"No translations for {user.TelegramNick}:{word.Word}");
                    await _addWordService.TranslateWordAndAddToDictionary(word.Word);
                }
                else {
                    var hasSimilar = word.RuTranslations.Any(
                        t => translations.Any(o => o.Equals(t.Word, StringComparison.InvariantCultureIgnoreCase)));
                    if (hasSimilar) continue;

                    Console.WriteLine($"Misstranslation for {user.TelegramNick}:{word.Word}");
                    await _localDictionaryService.AppendRuTranslation(
                        word.Word,
                        word.RuTranslations.SelectToArray(t => (t.Word, t.Examples.SelectToArray(e => e.ExampleId))));
                }
            }
        }
    }
    public async Task InitializeFreqWords()
    {
        var words = await GetFreqWords();
        await _frequentWordsRepo.Add(words);
    }

    public async Task<List<FrequentWord>> GetFreqWords()
    {
        var sets = await _learningSetService.GetAllSets();
        var orderedSets = new[]
        {
            sets.Single(s => s.ShortName == "top100"),
            sets.Single(s => s.ShortName == "top300"),
            sets.Single(s => s.ShortName == "top600"),
            sets.Single(s => s.ShortName == "top1000"),
            sets.Single(s => s.ShortName == "top1500"),
            sets.Single(s => s.ShortName == "top2000"),
            sets.Single(s => s.ShortName == "top3000"),
            sets.Single(s => s.ShortName == "top4000"),
            sets.Single(s => s.ShortName == "top5000"),
        };
        var allWords = orderedSets.SelectMany(r => r.Words).ToArray();
        var allFrequentWord = new List<FrequentWord>();
        var number = 0;
        var allWordsNumber = 0;
        while (allWordsNumber<allWords.Length)
        {
            number++;
            if(number<15)
            {
                AddEmptyWord();
            }
            else if (number < 100)
            {
                if(number%10 == 0)
                    AddEmptyWord();
                else
                    AddNextFrequentWord();
            }
            else if (number < 1000)
            {
                if(number%20 == 0)
                    AddEmptyWord();
                else
                    AddNextFrequentWord();
            }
            else
            {
                if(number%100 == 0)
                    AddEmptyWord();
                else
                    AddNextFrequentWord();
            }
            
        }
        /*
         *
              [5]: "top100"
              [2]: "top300"
              [4]: "top600"
              [0]: "top1000"
              [3]: "top1500"
              [1]: "top2000"
              [8]: "top3000"
              [6]: "top4000"
              [7]: "top5000"
         */
        return allFrequentWord;

        void AddEmptyWord()
        {
            allFrequentWord.Add(new FrequentWord
            {
                Id = ObjectId.GenerateNewId(),
                OrderNumber = number,
            });
        }

        void AddNextFrequentWord()
        {
            var freqWord = allWords[allWordsNumber];
            allFrequentWord.Add(new FrequentWord
            {
                Id = ObjectId.GenerateNewId(),
                OrderNumber = number,
                Word = freqWord.Word,
                AllowedExamples = freqWord.AllowedExamples.ToArray(),
                AllowedTranslations = freqWord.AllowedTranslations.ToArray(),
            });
            allWordsNumber++;
        }
    }
}