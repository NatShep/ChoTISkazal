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
        
        await esService.CreateLearningSet(
            new LearningSetDescription(
                "top2000", "The 1500-2000 most frequent words", "Частотные слова 1500..2000", "", "",
                ChaosHelper.LoadJson<List<EssentialWord>>(essentialPath).Skip(1500).Take(500).ToList()));
        await esService.CreateLearningSet(
            new LearningSetDescription(
                "top3000", "The 2000-3000 most frequent words", "Частотные слова 2000..3000", "", "",
                ChaosHelper.LoadJson<List<EssentialWord>>(essentialPath).Skip(2000).Take(1000).ToList()));
        await esService.CreateLearningSet(
            new LearningSetDescription(
                "top4000", "The 3000-4000 most frequent words", "Частотные слова 3000..4000", "", "",
                ChaosHelper.LoadJson<List<EssentialWord>>(essentialPath).Skip(3000).Take(1000).ToList()));
        await esService.CreateLearningSet(
            new LearningSetDescription(
                "top5000", "The 4000-5000 most frequent words", "Частотные слова 4000..5000", "", "",
                ChaosHelper.LoadJson<List<EssentialWord>>(essentialPath).Skip(4000).Take(1000).ToList()));
    }

    private static BotSettings ReadConfiguration() {
        try
        {
            var set = new BotSettings();
            Console.WriteLine("DEBUG SETTINGS APPLIED");
            set.MongoConnectionString ="<key>";
            set.MongoDbName = "SayWhatDb";
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