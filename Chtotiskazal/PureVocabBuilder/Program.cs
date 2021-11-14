using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot;
using MongoDB.Driver;
using SayWhat.Bll;
using SayWhat.Bll.Dto;
using SayWhat.Bll.Services;
using SayWhat.Bll.Yapi;
using SayWhat.MongoDAL.Dictionary;
using SayWhat.MongoDAL.Examples;
using SayWhat.MongoDAL.QuestionMetrics;
using SayWhat.MongoDAL.Users;
using SayWhat.MongoDAL.Words;

namespace PureVocabBuilder {

class Program {
    private static UsersWordsService _userWordService;
    private static LocalDictionaryService _localDictionaryService;
    private static UserService _userService;
    private static LearningSetService _learningSetService;
    private static AddWordService _addWordService;
    private static ExamplesRepo _examplesRepo;
    private static LocalDictionaryRepo _localDictionaryRepo;
    private static YandexDictionaryApiClient _yandexDictionaryClient;
    private static LearningSetsRepo _learningSetRepo;

    static async Task Main(string[] args) {
        Initialize();


        var essentialPath =
            "/Users/iurii.sukhanov/Desktop/Features/Buldozerowords/Zip/Final.essential";

        var esService = new EssentialService(_examplesRepo, _localDictionaryRepo, _addWordService, _learningSetRepo);

        //var words = await esService.MergeEssentials(OpTools.Load<List<EssentialWord>>(essentialPath).Take(1000).ToList());
        await esService.CreateLearningSet(
            new LearningSetDescription(
                "top100", "The 100 most frequent words", "100 самых частых слов", "", "",
                ChaosHelper.LoadJson<List<EssentialWord>>(essentialPath).Skip(0).Take(100).ToList()));
        await esService.CreateLearningSet(
            new LearningSetDescription(
                "top300", "The 300 most frequent words", "300 самых частых слов", "", "",
                ChaosHelper.LoadJson<List<EssentialWord>>(essentialPath).Skip(100).Take(200).ToList()));
        await esService.CreateLearningSet(
            new LearningSetDescription(
                "top600", "The 600 most frequent words", "600 самых частых слов", "", "",
                ChaosHelper.LoadJson<List<EssentialWord>>(essentialPath).Skip(300).Take(300).ToList()));
        await esService.CreateLearningSet(
            new LearningSetDescription(
                "top1000", "The 1000 most frequent words", "600 самых частых слов", "", "",
                ChaosHelper.LoadJson<List<EssentialWord>>(essentialPath).Skip(600).Take(400).ToList()));
        await esService.CreateLearningSet(
            new LearningSetDescription(
                "top1500", "The 1500 most frequent words", "1500 самых частых слов", "", "",
                ChaosHelper.LoadJson<List<EssentialWord>>(essentialPath).Skip(1000).Take(500).ToList()));
    }

    private static BotSettings ReadConfiguration() {
        try
        {
            var set = new BotSettings();
            Console.WriteLine("DEBUG SETTINGS APPLIED");
            set.MongoConnectionString ="mongodb://localhost:27017/";
            set.MongoDbName = "backuped";
            set.YadicapiKey = "<key>";
            set.YadicapiTimeout = TimeSpan.FromSeconds(5);


            return set;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private static void Initialize() {
        var settings = ReadConfiguration();


        _yandexDictionaryClient = new YandexDictionaryApiClient(settings.YadicapiKey, settings.YadicapiTimeout);
        var client = new MongoClient(settings.MongoConnectionString);
        var db = client.GetDatabase(settings.MongoDbName);

        var userWordRepo = new UserWordsRepo(db);
        _localDictionaryRepo = new LocalDictionaryRepo(db);
        var userRepo = new UsersRepo(db);
        _examplesRepo = new ExamplesRepo(db);
        var questionMetricsRepo = new QuestionMetricRepo(db);
        _learningSetRepo = new LearningSetsRepo(db);
        if (!userRepo.GetCount().Wait(10000))
        {
            throw new TimeoutException($"Could not connect to mongo db at {settings.MongoConnectionString}}}");
        }

        userWordRepo.UpdateDb();
        _localDictionaryRepo.UpdateDb();
        userRepo.UpdateDb();
        _examplesRepo.UpdateDb();
        questionMetricsRepo.UpdateDb();
        _learningSetRepo.UpdateDb();

        Botlog.QuestionMetricRepo = questionMetricsRepo;

        _userWordService = new UsersWordsService(userWordRepo, _examplesRepo);
        _localDictionaryService = new LocalDictionaryService(_localDictionaryRepo, _examplesRepo);
        _userService = new UserService(userRepo);
        _learningSetService = new LearningSetService(_learningSetRepo);
        _addWordService = new AddWordService(
            _userWordService,
            _yandexDictionaryClient,
            _localDictionaryService,
            _userService);
    }
}

}