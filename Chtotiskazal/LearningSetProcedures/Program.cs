using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Chotiskazal.Bot;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using SayWhat.Bll;
using SayWhat.Bll.Services;
using SayWhat.Bll.Yapi;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Dictionary;
using SayWhat.MongoDAL.Examples;
using SayWhat.MongoDAL.QuestionMetrics;
using SayWhat.MongoDAL.Users;
using SayWhat.MongoDAL.WordKits;
using SayWhat.MongoDAL.Words;

namespace LearningSetProcedures {

class Program {
    private static UsersWordsService _userWordService;
    private static LocalDictionaryService _localDictionaryService;
    private static UserService _userService;
    private static LearningSetService _learningSetService;
    private static AddWordService _addWordService;

    static async Task Main(string[] args) {
        
        Initialize();
        var vocaPath = "/Users/iurii.sukhanov/Desktop/Features/Buldozerowords/vocab.buldo";
        //var vocab = VocabFileTools.Load(vocaPath);
        //await VocabFileTools.SaveToMongo(_localDictionaryService,vocab);
        
        var all = await VocabFileTools.GetFromMongo(_localDictionaryService);
        VocabFileTools.Save(all, vocaPath);

        var path = "/Users/iurii.sukhanov/Desktop/Features/Buldozerowords/prepared_words";
        var pairs = ReadPairs(path, 0, 5000);//.Randomize().ToList();
        int delay = 1000;
        await EnsureDownloaded(pairs, 500);
        await CreateLearningSet(pairs);
    }

    private static BotSettings ReadConfiguration() {
        try
        {
            var set = new BotSettings();
            Console.WriteLine("DEBUG SETTINGS APPLIED");
            set.TelegramToken = "1410506895:AAH2Qy4yRBJ8b_9zkqD0z3B-_BUoezBdbXU";
            //   set.TelegramToken = "1432654477:AAE3j13y69yhLxNIS6JYGbZDfhIDrcfgzCs";
            set.MongoConnectionString = "mongodb://localhost:27017/";
            set.MongoDbName = "swdumbp";
            set.YadicapiKey = "dict.1.1.20200117T131333Z.11b4410034057f30.cd96b9ccbc87c4d9036dae64ba539fc4644ab33d";
            set.YadicapiTimeout = TimeSpan.FromSeconds(5);
            set.BotHelperToken = "1480472120:AAEXpltL9rrcgb3LE9sLWDeQrrXL4jVz1t8";
            set.ControlPanelChatId = "326823645";

            return set;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }


    private static async Task EnsureDownloaded(List<(string, string)> pairs, int delay) {
        foreach (var (en, ru) in pairs)
        {
            Console.WriteLine($"Translating {en}... ");
            var translation = await _addWordService.GetOrDownloadTranslation(en);
            Console.WriteLine($"{translation.Count} translations found");
            Thread.Sleep(delay);
        }
    }

    private static async Task CreateLearningSet(List<(string, string)> pairs) {
        LearningSet learningSet = new LearningSet() {
            Enabled = true,
            EnDescription = "First 100 words",
            EnName = "100",
            Id = ObjectId.GenerateNewId(),
            Passed = 0,
            RuDescription = "Первые 100 слов",
            Used = 0,
            Words = new List<WordInLearningSet>()
        };
        foreach (var (en, ru) in pairs)
        {
            Console.Write($"translating {en}... ");
            var translation = await _addWordService.GetOrDownloadTranslation(en);
            Console.WriteLine($"found {translation.Count} translations");
            var correcttrans = translation.FirstOrDefault(
                t => t.TranslatedText.Equals(ru, StringComparison.InvariantCultureIgnoreCase));
            if (correcttrans == null)
            {
                Console.WriteLine("TRANSLATION NOT FOUND");
            }

            learningSet.Words.Add(
                new WordInLearningSet {
                    AllowedExamples = new ObjectId[0],
                    AllowedTranslations = new[] { correcttrans.TranslatedText }
                });
        }

        await _learningSetService.Add(learningSet);
    }

    private static void Initialize() {
        var settings = ReadConfiguration();
        
        
        var yandexDictionaryClient = new YandexDictionaryApiClient(settings.YadicapiKey, settings.YadicapiTimeout);
        var client = new MongoClient(settings.MongoConnectionString);
        var db = client.GetDatabase(settings.MongoDbName);

        var userWordRepo = new UserWordsRepo(db);
        var dictionaryRepo = new DictionaryRepo(db);
        var userRepo = new UsersRepo(db);
        var examplesRepo = new ExamplesRepo(db);
        var questionMetricsRepo = new QuestionMetricRepo(db);
        var learningSetsRepo = new LearningSetsRepo(db);
        if (!userRepo.GetCount().Wait(1000))
        {
            throw new TimeoutException($"Could not connect to mongo db at {settings.MongoConnectionString}}}");
        }
        userWordRepo.UpdateDb();
        dictionaryRepo.UpdateDb();
        userRepo.UpdateDb();
        examplesRepo.UpdateDb();
        questionMetricsRepo.UpdateDb();
        learningSetsRepo.UpdateDb();

        Botlog.QuestionMetricRepo = questionMetricsRepo;

        _userWordService = new UsersWordsService(userWordRepo, examplesRepo);
        _localDictionaryService = new LocalDictionaryService(dictionaryRepo, examplesRepo);
        _userService = new UserService(userRepo);
        _learningSetService = new LearningSetService(learningSetsRepo);
        _addWordService = new AddWordService(
            _userWordService,
            yandexDictionaryClient,
            _localDictionaryService,
            _userService);
    }

    static List<(string, string)> ReadPairs(string path, int offset, int count) {
        var lines = File.ReadAllLines(path);
        var ans = new List<(string, string)>();
        foreach (string line in lines.Skip(offset))
        {
            if (ans.Count >= count)
                break;
            var splitted = line.Split("\t");
            if (splitted.Length == 2)
                ans.Add((splitted[0], splitted[1]));
        }

        return ans;
    }
}

}