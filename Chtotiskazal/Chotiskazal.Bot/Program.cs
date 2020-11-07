using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Chotiskazal.Api.Services;
using Chotiskazal.ConsoleTesting.Services;
using Chotiskazal.Dal.Migrations;
using Chotiskazal.Dal.Repo;
using Chotiskazal.Dal.Services;
using Chotiskazal.LogicR.yapi;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Args;


namespace Chotiskazal.Bot
{
    class Program
    {
        private const string ApiToken = "1432654477:AAE3j13y69yhLxNIS6JYGbZDfhIDrcfgzCs";
        static TelegramBotClient _botClient;
        static ConcurrentDictionary<long, ChatRoomFlow> _chats = new ConcurrentDictionary<long,ChatRoomFlow>();
        private static AddWordService addWordService;
        private static AuthorizeService authorizeService;
        private static ExamService examService;
        
        

        static void Main()
        {
            TaskScheduler.UnobservedTaskException +=
                (sender, args) => Console.WriteLine($"Unobserved ex {args.Exception}");
            
            
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
            IConfigurationRoot configuration = builder.Build();

            //TODO inkapsulate configuration to Chotiskazal.Api/Services
            
            var dbFileName = configuration.GetSection("wordDb").Value;
            
            var yadicapiKey = configuration.GetSection("yadicapi").GetSection("key").Value;
            var yadicapiTimeout = TimeSpan.FromSeconds(5);

            var yatransapiKey = configuration.GetSection("yatransapi").GetSection("key").Value;
            var yatransapiTimeout = TimeSpan.FromSeconds(5);
            
            addWordService = new AddWordService(
                new UsersWordsService(new UserWordsRepo(dbFileName)), 
                new YandexDictionaryApiClient(yadicapiKey,yadicapiTimeout), 
                new YandexTranslateApiClient(yatransapiKey,yatransapiTimeout),
                new DictionaryService(new DictionaryRepository(dbFileName)));
            
            examService=new ExamService(
                new ExamsAndMetricService(new ExamsAndMetricsRepo(dbFileName)),
                new DictionaryService(new DictionaryRepository(dbFileName)),
                new UsersWordsService(new UserWordsRepo(dbFileName))
            );
            authorizeService = new AuthorizeService(new UserService(new UserRepo(dbFileName)));
  
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

        static ChatRoomFlow GetOrCreate(Telegram.Bot.Types.Chat chat)
        {
            if (_chats.TryGetValue(chat.Id, out var existedChatRoom))
                return existedChatRoom;

            var newChat = new Chat(_botClient, chat);
            var newChatRoom = new ChatRoomFlow(newChat)
            {
                ExamSrvc = examService,
                AddWordSrvc = addWordService,
            };
            
            var task = newChatRoom.Run();
            
            task.ContinueWith((t) => Botlog.Write($"faulted {t.Exception}"), TaskContinuationOptions.OnlyOnFaulted);
            _chats.TryAdd(chat.Id, newChatRoom);
            return null;
        }

        static async void BotClientOnOnUpdate(object? sender, UpdateEventArgs e)
        {
            
            Botlog.Write($"Got query: {e.Update.Type}");

            if (e.Update.Message != null)
            {
                var chatRoom = GetOrCreate(e.Update.Message.Chat);
                chatRoom?.Chat.HandleUpdate(e.Update);

            }
            else if (e.Update.CallbackQuery != null)
            {
                var chatRoom = GetOrCreate(e.Update.CallbackQuery.Message.Chat);
                chatRoom?.Chat.HandleUpdate(e.Update);
                
                await _botClient.AnswerCallbackQueryAsync(e.Update.CallbackQuery.Id);
            }
        }

        static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Text != null)
            {
                Botlog.Write($"Received a text message in chat {e.Message.Chat.Id}.");

                //await botClient.SendTextMessageAsync(
                //    chatId: e.Message.Chat,
                //    text: "You said:\n" + e.Message.Text
                //);
            }
        }
    }
}

