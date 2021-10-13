using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.ChatFlows;
using Chotiskazal.Bot.InterfaceLang;
using Chotiskazal.Bot.Questions;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using SayWhat.Bll;
using SayWhat.Bll.Services;
using SayWhat.Bll.Yapi;
using SayWhat.MongoDAL.Dictionary;
using SayWhat.MongoDAL.Examples;
using SayWhat.MongoDAL.QuestionMetrics;
using SayWhat.MongoDAL.Users;
using SayWhat.MongoDAL.Words;
using Serilog;
using Serilog.Events;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using TelegramSink;


namespace Chotiskazal.Bot
{
    static class Program {
        private static TelegramBotClient _botClient;
        private static readonly ConcurrentDictionary<long, MainFlow> Chats = new ConcurrentDictionary<long,MainFlow>();
        private static AddWordService _addWordService;
        private static DictionaryService _dictionaryService;
        private static UsersWordsService _userWordService;
        private static BotSettings _settings;
        private static UserService _userService;

        private static void Main()
        {
            TaskScheduler.UnobservedTaskException +=
                (sender, args) => Console.WriteLine($"Unobserved ex {args.Exception}");
            
            _settings = ReadConfiguration(substitudeDebugConfig: true);
            var yandexDictionaryClient   = new YandexDictionaryApiClient(_settings.YadicapiKey,   _settings.YadicapiTimeout);
            var client = new MongoClient(_settings.MongoConnectionString);
            var db = client.GetDatabase(_settings.MongoDbName);
            //StatsMigrator.Do(db).Wait();
            var userWordRepo   = new UserWordsRepo(db);
            var dictionaryRepo = new DictionaryRepo(db);
            var userRepo       = new UsersRepo(db);
            var examplesRepo   = new ExamplesRepo(db);
            var questionMetricsRepo = new QuestionMetricRepo(db);
            var learningSetsRepo = new LearningSetsRepo(db);
            userWordRepo.UpdateDb();
            dictionaryRepo.UpdateDb();
            userRepo.UpdateDb();
            examplesRepo.UpdateDb();
            questionMetricsRepo.UpdateDb();
            learningSetsRepo.UpdateDb();
            
            Botlog.QuestionMetricRepo = questionMetricsRepo;
            
            _userWordService      = new UsersWordsService(userWordRepo, examplesRepo);
            _dictionaryService    = new DictionaryService(dictionaryRepo,examplesRepo);
            _userService          = new UserService(userRepo);
            
            _addWordService = new AddWordService(
                _userWordService,
                yandexDictionaryClient,
                _dictionaryService, 
                _userService);
            
            QuestionSelector.Singletone = new QuestionSelector(_dictionaryService);
    
            _botClient = new TelegramBotClient(_settings.TelegramToken);
            
            Botlog.CreateTelegramLogger(_settings.BotHelperToken,_settings.ControlPanelChatId);

            var me = _botClient.GetMeAsync().Result;
            
            Botlog.WriteInfo($"Waking up. I am {me.Id}:{me.Username} ",true);

            var allUpdates =_botClient.GetUpdatesAsync().Result;
            Botlog.WriteInfo($"{allUpdates.Length} messages were missed");

            foreach (var update in allUpdates) 
                OnBotWokeUp(update);
            if (allUpdates.Any())
            {
                _botClient.MessageOffset = allUpdates.Last().Id + 1;
                Botlog.WriteInfo($"{Chats.Count} users were waiting for us");
            }
            _botClient.OnUpdate+= BotClientOnOnUpdate;
            _botClient.OnMessage += Bot_OnMessage;
            
            _botClient.StartReceiving();
            Botlog.WriteInfo($"... and here i go!",false);
            // workaround for infinity awaiting
             new TaskCompletionSource<bool>().Task.Wait();
             // it will never happens
             _botClient.StopReceiving();
        }

        private static BotSettings ReadConfiguration(bool substitudeDebugConfig)
        {
            try
            {
                var builder = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
                var configuration = builder.Build();
                
                var set = new BotSettings(configuration);
                if (substitudeDebugConfig)
                {
                    Console.WriteLine("DEBUG SETTINGS APPLIED");
                    set.TelegramToken = "1410506895:AAH2Qy4yRBJ8b_9zkqD0z3B-_BUoezBdbXU";
                        //   set.TelegramToken = "1432654477:AAE3j13y69yhLxNIS6JYGbZDfhIDrcfgzCs";
                    set.MongoConnectionString = "mongodb://localhost:27017/";
                    set.MongoDbName = "swdumbp";
                    set.BotHelperToken = "1480472120:AAEXpltL9rrcgb3LE9sLWDeQrrXL4jVz1t8";
                    set.ControlPanelChatId = "326823645";
                }

                return set;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }
        private static void OnBotWokeUp(Update update)
        {
            Telegram.Bot.Types.Chat chat = null;
            Telegram.Bot.Types.User user = null;
            try
            {
               
                if (update?.Message != null)
                {
                    chat = update.Message.Chat;
                    user = update.Message.From;
                }
                else if (update?.CallbackQuery != null)
                {
                    chat = update.CallbackQuery.Message.Chat;
                    user = update.CallbackQuery.From;
                }
                else
                    return;

                if (Chats.TryGetValue(chat.Id, out _))
                    return;
            
                var chatRoom = CreateChatRoom(chat);
                if (chatRoom == null)
                {
                    return;
                }
                chatRoom.ChatIo.SendMessageAsync( new EnglishTexts().DidYouWriteSomething).Wait();
                chatRoom.ChatIo.OnUpdate(
                    new Update {Message = new Message {Text = "/start", From = user}});
                
                RunChatRoom(chat, chatRoom);
            }
            catch (Exception e)
            {
                Botlog.WriteError(chat?.Id, "WokeUpfailed"+e, true);
            }
        }
        private static MainFlow GetOrCreate(Telegram.Bot.Types.Chat chat)
        {
            if (Chats.TryGetValue(chat.Id, out var existedChatRoom))
                return existedChatRoom;
            
            var newChatRoom = CreateChatRoom(chat);
            if (newChatRoom == null)
            {
                Console.WriteLine($"Chat {chat.Id} rejected");
                return null;
            }
            Console.WriteLine($"Chat {chat.Id} joined");
            RunChatRoom(chat, newChatRoom);
            return newChatRoom;
        }
        private static void RunChatRoom(Chat chat, MainFlow newMain)
        {
            var task = newMain.Run();
            task.ContinueWith(
                (t) =>
                {
                    Botlog.WriteError(chat.Id, $"Faulted {t.Exception}",true);
                    Chats.TryRemove(chat.Id, out _);
                }, TaskContinuationOptions.OnlyOnFaulted);
        }
        private static void BotClientOnOnUpdate(object sender, UpdateEventArgs e)
        {
            long? chatId = null;
            try
            {
                Botlog.WriteInfo($"Trying to got query: {e.Update.Type}...");

                if (e.Update.Message != null)
                {
                    chatId = e.Update.Message.Chat?.Id;
                    Botlog.WriteInfo($"Got query: {e.Update.Type}",chatId.ToString());
                    var chatRoom = GetOrCreate(e.Update.Message.Chat);
                    chatRoom?.ChatIo.OnUpdate(e.Update);
                }
                else if (e.Update.CallbackQuery != null)
                {
                    chatId = e.Update.CallbackQuery.Message.Chat?.Id;
                    Botlog.WriteInfo($"Got query: {e.Update.Type}",chatId.ToString());

                    var chatRoom = GetOrCreate(e.Update.CallbackQuery.Message.Chat);
                    chatRoom?.ChatIo.OnUpdate(e.Update);
                }
            }
            catch (Exception)
            {
                Botlog.WriteError(chatId, $"BotClientOnOnUpdate Failed: {e.Update.Type}",true);
            }
        }

        private static void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Text != null)
            {
                Botlog.WriteInfo($"Received a text message to chat {e.Message.Chat.Id}.",e.Message.Chat.Id.ToString());
            }
        }
        
        private static MainFlow CreateChatRoom(Chat chat)
        {
            var newChatRoom = new MainFlow(
                new ChatIO(_botClient, chat), 
                new TelegramUserInfo(chat.Id, chat.FirstName, chat.LastName, chat.Username), 
                _settings,
                _addWordService,
                _userWordService, 
                _userService);
            Chats.TryAdd(chat.Id, newChatRoom);
            return newChatRoom;
        }
    }
}

