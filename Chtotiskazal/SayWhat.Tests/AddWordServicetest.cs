using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SayWhat.Bll.Dto;
using SayWhat.Bll.Services;
using SayWhat.Bll.Yapi;
using SayWhat.MongoDAL.Dictionary;
using SayWhat.MongoDAL.Examples;
using SayWhat.MongoDAL.Users;
using SayWhat.MongoDAL.Words;

namespace SayWhat.MongoDAL.Tests {

public class AddWordServicetest {
    private AddWordService _service;
    private UsersRepo _userRepo;
    private UserWordsRepo _userWordsRepo;
    private ExamplesRepo _exRepo;

    [SetUp]
    public void Intitalize()
    {
        MongoTestHelper.DropAllCollections();

        _userRepo = new UsersRepo(MongoTestHelper.Database);
        _userWordsRepo = new UserWordsRepo(MongoTestHelper.Database);
        _exRepo = new ExamplesRepo(MongoTestHelper.Database);
        var localDicrepo = new LocalDictionaryRepo(MongoTestHelper.Database);
        var userWordsService = new UsersWordsService(_userWordsRepo, _exRepo);
        var localDicService = new LocalDictionaryService(localDicrepo, _exRepo);
        var userService = new UserService(_userRepo);
        _service =new AddWordService(userWordsService, new YandexDictionaryApiClient("", TimeSpan.Zero), localDicService, userService);
        
    }

    [Test]
    public async Task RemoveWordsTest() {
        var user = new UserModel(null, "vasa", "pupkin", "@vasa", UserSource.Telegram);
        await _userRepo.Add(user);
        await _service.AddTranslationToUser(
            user, new Translation("piss", "моча", "", TranslationDirection.EnRu, TranslationSource.Manual));
        await _service.AddTranslationToUser(
            user, new Translation("piss", "писать", "", TranslationDirection.EnRu, TranslationSource.Manual));

        await _service.RemoveTranslationFromUser(
            user, new Translation("piss", "писать", "", TranslationDirection.EnRu, TranslationSource.Manual));
        
        var word = await _userWordsRepo.GetWordOrDefault(user, "piss");
        
        Assert.AreEqual(1, word.RuTranslations.Length);
        Assert.AreEqual("моча", word.RuTranslations[0].Word);
        
        
        await _service.RemoveTranslationFromUser(
            user, new Translation("piss", "моча", "", TranslationDirection.EnRu, TranslationSource.Manual));
        Assert.IsNull(await _userWordsRepo.GetWordOrDefault(user, "piss"));
    }
}

}