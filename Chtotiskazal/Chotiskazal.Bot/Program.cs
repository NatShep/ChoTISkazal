using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using SayWhat.Bll.Services;
using SayWhat.Bll.Yapi;
using SayWhat.MongoDAL.Dictionary;
using SayWhat.MongoDAL.Examples;
using SayWhat.MongoDAL.Users;
using SayWhat.MongoDAL.Words;
using Telegram.Bot;
using Telegram.Bot.Args;


namespace Chotiskazal.Bot
{
    static class Program {
        private static TelegramBotClient _botClient;
        private static readonly ConcurrentDictionary<long, ChatRoomFlow> Chats = new ConcurrentDictionary<long,ChatRoomFlow>();
        private static AddWordService _addWordService;
        private static DictionaryService _dictionaryService;
        private static AuthorizationService _authorizationService;
        private static UsersWordsService _userWordService;
        private static MetricService _metricService;
        private static void Main()
        {
            TaskScheduler.UnobservedTaskException +=
                (sender, args) => Console.WriteLine($"Unobserved ex {args.Exception}");
            
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
            var configuration = builder.Build();
            
            var settings = new BotSettings(configuration);

            var yandexDictionaryClient = new YandexDictionaryApiClient(settings.YadicapiKey, settings.YadicapiTimeout);
            var yandexTranslateApiClient = new YandexTranslateApiClient(settings.YatransapiKey, settings.YatransapiTimeout); 
                
            var client = new MongoClient(settings.MongoConnectionString);
            var db = client.GetDatabase("SayWhatDb");

            var userWordRepo   = new UserWordsRepo(db);
            var dictionaryRepo = new DictionaryRepo(db);
            var userRepo       = new UsersRepo(db);
            var examplesRepo   = new ExamplesRepo(db);
            
            userWordRepo.UpdateDb();
            dictionaryRepo.UpdateDb();
            userRepo.UpdateDb();
            examplesRepo.UpdateDb();
            
            _userWordService      = new UsersWordsService(userWordRepo, examplesRepo);
            _dictionaryService    = new DictionaryService(dictionaryRepo,examplesRepo);
            _authorizationService = new AuthorizationService(new UserService(userRepo));

            _metricService  = new MetricService();
            _addWordService = new AddWordService(
                _userWordService, 
                yandexDictionaryClient,
                yandexTranslateApiClient,
                _dictionaryService);
            
            ExamSelector.Singletone = new ExamSelector(_dictionaryService);
      
            Console.WriteLine("Dic started");
    
            _botClient = new TelegramBotClient(settings.TelegramToken);
            var me = _botClient.GetMeAsync().Result;
            Console.WriteLine(
                $"Hello, World! I am user {me.Id} and my name is {me.FirstName}."
            );

            _botClient.OnUpdate+= BotClientOnOnUpdate;
            _botClient.OnMessage += Bot_OnMessage;
            
            _botClient.StartReceiving();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            
            _botClient.StopReceiving();
        }

        private static ChatRoomFlow GetOrCreate(Telegram.Bot.Types.Chat chat)
        {
            if (Chats.TryGetValue(chat.Id, out var existedChatRoom))
                return existedChatRoom;

            var newChatRoom = new ChatRoomFlow(
                new ChatIO(_botClient, chat), 
                chat.FirstName,
                _addWordService,
                _userWordService, 
                _metricService, 
                _authorizationService);
            
            var task = newChatRoom.Run();

            task.ContinueWith((t) => Botlog.Error(chat.Id,  $"Faulted {t.Exception}"), TaskContinuationOptions.OnlyOnFaulted);
            Chats.TryAdd(chat.Id, newChatRoom);
            return newChatRoom;
        }

        private static async void BotClientOnOnUpdate(object sender, UpdateEventArgs e)
        {
            long? chatId = null;
            try
            {
                Botlog.Write($"Got query: {e.Update.Type}");

                if (e.Update.Message != null)
                {
                    chatId = e.Update.Message.Chat?.Id;
                    var chatRoom = GetOrCreate(e.Update.Message.Chat);
                    chatRoom?.ChatIo.HandleUpdate(e.Update);

                }
                else if (e.Update.CallbackQuery != null)
                {
                    chatId = e.Update.CallbackQuery.Message.Chat?.Id;
                    var chatRoom = GetOrCreate(e.Update.CallbackQuery.Message.Chat);
                    chatRoom?.ChatIo.HandleUpdate(e.Update);
                    await _botClient.AnswerCallbackQueryAsync(e.Update.CallbackQuery.Id);
                }
            }
            catch (Exception error)
            {
                Botlog.Error(chatId, $"BotClientOnOnUpdate Failed: {e.Update.Type} {error}");
            }
        }

        private static void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Text != null)
            {
                Botlog.Write($"Received a text message in chat {e.Message.Chat.Id}.");
            }
        }
    }
}

