using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Chotiskazal.Bot.ChatFlows;
using Chotiskazal.Bot.Jobs;
using Chotiskazal.Bot.Questions;
using Chotiskazal.Bot.Texts;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using SayWhat.Bll;
using SayWhat.Bll.Services;
using SayWhat.Bll.Statistics;
using SayWhat.Bll.Yapi;
using SayWhat.MongoDAL.Dictionary;
using SayWhat.MongoDAL.Examples;
using SayWhat.MongoDAL.LearningSets;
using SayWhat.MongoDAL.LongDataForTranslationButton;
using SayWhat.MongoDAL.QuestionMetrics;
using SayWhat.MongoDAL.Users;
using SayWhat.MongoDAL.Words;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;


namespace Chotiskazal.Bot;

static class Program {
    private static TelegramBotClient _botClient;
    private static readonly ConcurrentDictionary<long, MainFlow> Chats = new();
    private static AddWordService _addWordService;
    private static ButtonCallbackDataService _buttonCallbackDataService;
    private static LocalDictionaryService _localDictionaryService;
    private static UsersWordsService _userWordService;
    private static BotSettings _settings;
    private static UserService _userService;
    private static LearningSetService _learningSetService;
    private static QuestionSelector _questionSelector;

    private static async Task Main() {
        TaskScheduler.UnobservedTaskException +=
            (sender, args) => Console.WriteLine($"Unobserved ex {args.Exception}");

        _settings = ReadConfiguration(substitudeDebugConfig: false);
        var yandexDictionaryClient = new YandexDictionaryApiClient(_settings.YadicapiKey, _settings.YadicapiTimeout);
        var client = new MongoClient(_settings.MongoConnectionString);
        var db = client.GetDatabase(_settings.MongoDbName);

        var userWordRepo = new UserWordsRepo(db);
        var dictionaryRepo = new LocalDictionaryRepo(db);
        var userRepo = new UsersRepo(db);
        var examplesRepo = new ExamplesRepo(db);
        var questionMetricsRepo = new QuestionMetricRepo(db);
        var learningSetsRepo = new LearningSetsRepo(db);
        var longDataForButtonRepo = new LongCallbackDataRepo(db);

        await userWordRepo.UpdateDb();
        await dictionaryRepo.UpdateDb();
        await userRepo.UpdateDb();
        await examplesRepo.UpdateDb();
        await questionMetricsRepo.UpdateDb();
        await learningSetsRepo.UpdateDb();

        Reporter.QuestionMetricRepo = questionMetricsRepo;

        _userWordService = new UsersWordsService(userWordRepo, examplesRepo);
        _localDictionaryService = new LocalDictionaryService(dictionaryRepo, examplesRepo);
        _userService = new UserService(userRepo);
        _learningSetService = new LearningSetService(learningSetsRepo);
        _addWordService = new AddWordService(
            _userWordService,
            yandexDictionaryClient,
            _localDictionaryService,
            _userService);
        _buttonCallbackDataService = new ButtonCallbackDataService(longDataForButtonRepo);

        _questionSelector = new QuestionSelector(_localDictionaryService);

        _botClient = new TelegramBotClient(_settings.TelegramToken);

        var telegramLogger = TelegramLogger.CreateLogger(_settings.BotHelperToken, _settings.ControlPanelChatId);

        Reporter.SetTelegramLogger(telegramLogger);

        var me = _botClient.GetMeAsync().Result;

        Reporter.ReportBotWokeup(me.Id, me.Username);

        _botClient.SetMyCommandsAsync(BotCommands.Descriptions).Wait();
        Update[] allUpdates = _botClient.GetUpdatesAsync().Result;

        Reporter.WriteInfo($"{allUpdates.Length} messages were missed");

        foreach (var update in allUpdates)
            OnBotWokeUp(update);
        var messageOffset = 0;
        if (allUpdates.Any()) {
            messageOffset = allUpdates.Last().Id + 1;
            Reporter.WriteInfo($"{Chats.Count} users were waiting for us");
        }
    
        //_botClient.OnUpdate += BotClientOnOnUpdate;
        //_botClient.OnMessage += Bot_OnMessage;

        //_botClient.StartReceiving();

        ReportSenderJob.Launch(TimeSpan.FromDays(1), telegramLogger);
        var remindJobTask = RemindSenderJob.Launch(_botClient, _userService, telegramLogger);
        var updateCurrentScoreJobTask = UpdateCurrentScoresJob.Launch(telegramLogger, _userService, _userWordService);
        var receiverOptions = new ReceiverOptions()
        {
            Offset = messageOffset,
            AllowedUpdates = new[]
            {
                UpdateType.Message,
                UpdateType.CallbackQuery,
                UpdateType.ChosenInlineResult,
                UpdateType.ShippingQuery,
                UpdateType.InlineQuery,
                UpdateType.PreCheckoutQuery,
            },
            ThrowPendingUpdates = true,
        };
        var updateHandler = new UpdateHandler();
        var receiveTask = _botClient.ReceiveAsync(
            updateHandler: updateHandler,
            receiverOptions: receiverOptions,
            cancellationToken: CancellationToken.None);
        Reporter.WriteInfo($"... and here i go!");
        // workaround for infinity awaiting
        new TaskCompletionSource<bool>().Task.Wait();
        
        //----
        await remindJobTask;
        await updateCurrentScoreJobTask;
        await receiveTask;
        // it will never happens
        //_botClient.StopReceiving();
    }

    private static BotSettings ReadConfiguration(bool substitudeDebugConfig) {
        try {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
            var configuration = builder.Build();

            var set = new BotSettings(configuration);
            if (substitudeDebugConfig) {
                Console.WriteLine("DEBUG SETTINGS APPLIED");
                set.MongoConnectionString = "mongodb://localhost:27017/";
                set.MongoDbName = "swdumbp";
            }

            return set;
        }
        catch (Exception e) {
            Console.WriteLine(e);
            throw;
        }
    }

    private static void OnBotWokeUp(Update update) {
        Chat chat = null;
        User user = null;
        try {
            if (update?.Message != null) {
                chat = update.Message.Chat;
                user = update.Message.From;
            }
            else if (update?.CallbackQuery != null) {
                chat = update.CallbackQuery.Message.Chat;
                user = update.CallbackQuery.From;
            }
            else
                return;

            if (Chats.TryGetValue(chat.Id, out _))
                return;

            var chatRoom = CreateChatRoom(chat);
            if (chatRoom == null) {
                return;
            }

            chatRoom.ChatIo.SendMessageAsync(new EnglishTexts().DidYouWriteSomething).Wait();
            chatRoom.ChatIo.OnUpdate(
                new Update { Message = new Message { Text = "/start", From = user } });

            RunChatRoom(chat, chatRoom);
        }
        catch (Exception e) {
            Reporter.ReportError(chat?.Id, "WokeUp failed", e);
        }
    }

    public static MainFlow GetOrCreate(Chat chat) {
        if (Chats.TryGetValue(chat.Id, out var existedChatRoom))
            return existedChatRoom;

        var newChatRoom = CreateChatRoom(chat);
        if (newChatRoom == null) {
            Console.WriteLine($"Chat {chat.Id} rejected");
            return null;
        }

        Console.WriteLine($"Chat {chat.Id} joined");
        RunChatRoom(chat, newChatRoom);
        return newChatRoom;
    }

    private static void RunChatRoom(Chat chat, MainFlow newMain) {
        var task = newMain.Run();
        task.ContinueWith(
            t => {
                Reporter.ReportError(chat.Id, $"Main chatroom failed", newMain.ChatIo?.TryGetChatHistory(),
                    t.Exception);
                Chats.TryRemove(chat.Id, out _);
            }, TaskContinuationOptions.OnlyOnFaulted);
    }

    private static MainFlow CreateChatRoom(Chat chat) {
        var newChatRoom = new MainFlow(
            new ChatIO(_botClient, chat),
            new TelegramUserInfo(chat.Id, chat.FirstName, chat.LastName, chat.Username),
            _settings,
            _addWordService,
            _userWordService,
            _userService,
            _localDictionaryService,
            _learningSetService,
            _buttonCallbackDataService,
            _questionSelector);
        Chats.TryAdd(chat.Id, newChatRoom);
        return newChatRoom;
    }

    public class UpdateHandler : IUpdateHandler {
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
            CancellationToken cancellationToken) {
            long? chatId = null;
            MainFlow chatRoom = null;
            try {
                Reporter.WriteInfo($"Trying to got query: {update.Type}...");

                if (update.Message != null) {
                    chatId = update.Message.Chat?.Id;
                    Reporter.WriteInfo($"Got query: {update.Type}", chatId.ToString());
                    chatRoom = GetOrCreate(update.Message.Chat);
                    chatRoom?.ChatIo.OnUpdate(update);
                    if (update.Message.Text != null)
                        Reporter.WriteInfo($"Received a text message to chat {update.Message.Chat.Id}.",
                            update.Message.Chat.ToString());
                }
                else if (update.CallbackQuery != null) {
                    chatId = update.CallbackQuery.Message.Chat?.Id;
                    Reporter.WriteInfo($"Got query: {update.Type}", chatId.ToString());

                    chatRoom = GetOrCreate(update.CallbackQuery.Message.Chat);
                    chatRoom?.ChatIo.OnUpdate(update);
                }
            }
            catch (Exception ex) {
                Reporter.ReportError(chatId, $"BotClientOnOnUpdate Failed: {update.Type}",
                    chatRoom?.ChatIo?.TryGetChatHistory(), ex);
            }
        }

        public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception,
            CancellationToken cancellationToken) {
            Reporter.ReportPollingErrorError(botClient.BotId, exception);
        }
    }
}