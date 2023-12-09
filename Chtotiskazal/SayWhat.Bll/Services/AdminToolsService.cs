using System;
using System.Linq;
using System.Threading.Tasks;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Users;
using SayWhat.MongoDAL.Words;

namespace SayWhat.Bll.Services;

/// <summary>
/// Операции для администрирования бота. Вызываются из вручную написанного кода
/// </summary>
public class AdminToolsService {
    private UsersRepo _userRepo;
    private UserWordsRepo _userWordRepo;
    private LocalDictionaryService _localDictionaryService;
    private AddWordService _addWordService;
    public AdminToolsService(UsersRepo userRepo, UserWordsRepo userWordRepo, LocalDictionaryService localDictionaryService, AddWordService addWordService) {
        _userRepo = userRepo;
        _userWordRepo = userWordRepo;
        _localDictionaryService = localDictionaryService;
        _addWordService = addWordService;
    }

    public async Task ReportForNotSynchronizedUserWordsAndLocalDictionary() {
        var allUsers = _userRepo.GetAll();

        foreach (var user in allUsers)
        {
            var allWords = await _userWordRepo.GetAllWords(user);
            foreach (var word in allWords)
            {
                var translations = await _localDictionaryService.GetAllTranslationWords(word.Word);
                if (!translations.Any())
                {
                    Console.WriteLine($"No translations for {user.TelegramNick}:{word.Word}");
                }
                else
                {
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

        foreach (var user in allUsers)
        {
            var allWords = await _userWordRepo.GetAllWords(user);
            foreach (var word in allWords)
            {
                var translations = await _localDictionaryService.GetAllTranslationWords(word.Word);
                if (!translations.Any())
                {
                    Console.WriteLine($"No translations for {user.TelegramNick}:{word.Word}");
                    await _addWordService.TranslateAndAddToDictionary(word.Word);
                }
                else
                {
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
}