using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Chotiskazal.Bot.Services;
using Chotiskazal.Dal.Migrations;
using Chotiskazal.Dal.Repo;
using Chotiskazal.Dal.Services;
using Chotiskazal.Dal.yapi;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Args;


namespace Chotiskazal.Bot
{
    static class Program
    {
        private const string ApiToken = "1432654477:AAE3j13y69yhLxNIS6JYGbZDfhIDrcfgzCs";
        private static TelegramBotClient _botClient;
        private static readonly ConcurrentDictionary<long, ChatRoomFlow> Chats = new ConcurrentDictionary<long,ChatRoomFlow>();
        private static AddWordService _addWordService;
        private static AuthorizeService _authorizeService;
        private static ExamService _examService;
        private static YaService _yaService;

        private static void Main()
        {
            TaskScheduler.UnobservedTaskException +=
                (sender, args) => Console.WriteLine($"Unobserved ex {args.Exception}");
            
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
            var configuration = builder.Build();

            var dbFileName = configuration.GetSection("wordDb").Value;

            if(dbFileName==null)
               throw new Exception("No dbFileName");

            var yadicapiKey = configuration.GetSection("yadicapi").GetSection("key").Value;
            var yadicapiTimeout = TimeSpan.FromSeconds(5);

            var yatransapiKey = configuration.GetSection("yatransapi").GetSection("key").Value;
            var yatransapiTimeout = TimeSpan.FromSeconds(5);

            var yandexDictionaryClient = new YandexDictionaryApiClient(yadicapiKey, yadicapiTimeout);
            var yandexTranslateApiClient = new YandexTranslateApiClient(yatransapiKey, yatransapiTimeout); 
                
            var userWordService = new UsersWordsService(new UserWordsRepo(dbFileName));
            var dictionaryService= new DictionaryService(new DictionaryRepository(dbFileName));
            var examsAndMetricService = new ExamsAndMetricService(new ExamsAndMetricsRepo(dbFileName));

            _yaService = new YaService(yandexDictionaryClient,yandexTranslateApiClient);
            _authorizeService = new AuthorizeService(new UserService(new UserRepo(dbFileName)));
            _addWordService = new AddWordService(
                userWordService, 
                yandexDictionaryClient,
                yandexTranslateApiClient,
                dictionaryService);
            _examService=new ExamService(
                examsAndMetricService,
                dictionaryService,
                userWordService);

            DoMigration.ApplyMigrations(dbFileName);
      
            Console.WriteLine("Dic started");
    
            _botClient = new TelegramBotClient(ApiToken);
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

            var newChat = new ChatIO(_botClient, chat);

            var newChatRoom = new ChatRoomFlow(newChat,chat.FirstName)
            {
                ExamSrvc = _examService,
                AddWordSrvc = _addWordService,
                AuthorizeSrvc = _authorizeService,
                YaSrvc = _yaService
                
            };
            
            var task = newChatRoom.Run();

            task.ContinueWith((t) => Botlog.Write($"faulted {t.Exception}"), TaskContinuationOptions.OnlyOnFaulted);
            Chats.TryAdd(chat.Id, newChatRoom);
         
            return null;
        }

        private static async void BotClientOnOnUpdate(object sender, UpdateEventArgs e)
        {
            try
            {
                Botlog.Write($"Got query: {e.Update.Type}");

                if (e.Update.Message != null)
                {
                    var chatRoom = GetOrCreate(e.Update.Message.Chat);
                    chatRoom?.ChatIo.HandleUpdate(e.Update);

                }
                else if (e.Update.CallbackQuery != null)
                {
                    var chatRoom = GetOrCreate(e.Update.CallbackQuery.Message.Chat);
                    chatRoom?.ChatIo.HandleUpdate(e.Update);
                    await _botClient.AnswerCallbackQueryAsync(e.Update.CallbackQuery.Id);
                }
            }
            catch (Exception error)
            {
                Botlog.Write($"BotClientOnOnUpdate Failed: {e.Update.Type} {error}");
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

