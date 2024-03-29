﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.CommandHandlers;
using Chotiskazal.Bot.Hooks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll;
using SayWhat.Bll.Services;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL.Users;

namespace Chotiskazal.Bot.ChatFlows;

public class MainFlow
{
    public MainFlow(
        ChatIO chatIo,
        TelegramUserInfo userInfo,
        BotSettings settings,
        AddWordService addWordsService,
        UsersWordsService usersWordsService,
        UserService userService,
        LocalDictionaryService localDictionaryService,
        LearningSetService learningSetService,
        ButtonCallbackDataService buttonCallbackDataService,
        MutualPhrasesService mutualPhrasesService,
        QuestionSelector questionSelector,
        FrequentWordService frequentWordService)
    {
        ChatIo = chatIo;
        _userInfo = userInfo;
        _settings = settings;
        _addWordsService = addWordsService;
        _usersWordsService = usersWordsService;
        _userService = userService;
        _localDictionaryService = localDictionaryService;
        _learningSetService = learningSetService;
        _buttonCallbackDataService = buttonCallbackDataService;
        _mutualPhrasesService = mutualPhrasesService;
        _questionSelector = questionSelector;
        _frequentWordService = frequentWordService;
    }

    private readonly AddWordService _addWordsService;
    private readonly UsersWordsService _usersWordsService;
    private readonly UserService _userService;
    private readonly LocalDictionaryService _localDictionaryService;
    private readonly ButtonCallbackDataService _buttonCallbackDataService;
    private readonly MutualPhrasesService _mutualPhrasesService;
    private readonly QuestionSelector _questionSelector;
    private readonly LearningSetService _learningSetService;
    private readonly TelegramUserInfo _userInfo;
    private readonly BotSettings _settings;
    private readonly FrequentWordService _frequentWordService;

    private TranslationSelectedUpdateHook _translationSelectedUpdateHook;
    private LeafWellKnownWordsUpdateHook _wellKnownWordsUpdateHook;
    private AddBotCommandHandler _addWordCommandHandler;
    public ChatIO ChatIo { get; }
    public UserModel User => Chat?.User;

    public async Task Run()
    {
        try
        {
            await Initialize();

            (string Command, IBotCommandHandler CommandHandler)? mainMenuCommandOrNull = null;

            while (true)
            {
                try
                {
                    //run main scenario or mainMenuCommand
                    await HandleMainScenario(mainMenuCommandOrNull);
                    mainMenuCommandOrNull = null;
                }
                catch (UserAFKException)
                {
                    return;
                }
                catch (ProcessInterruptedWithMenuCommand e)
                {
                    Reporter.ReportCommand(e.Command, ChatIo.ChatId.Identifier ?? 0L);
                    //main scenario may be interrupted with main menu command
                    mainMenuCommandOrNull = (e.Command, e.CommandHandler);
                }
                catch (Exception e)
                {
                    Reporter.ReportError(ChatIo.ChatId.Identifier, $"Main failure for{ChatIo.ChatId.Username}",
                        ChatIo?.TryGetChatHistory(), e);
                    await ChatIo.SendMessageAsync(Chat.Texts.OopsSomethingGoesWrong);
                    throw;
                }
            }
        }
        catch (Exception e)
        {
            Reporter.ReportError(ChatIo?.ChatId?.Identifier, $"Fatal on run", ChatIo?.TryGetChatHistory(), e);
            throw;
        }
    }

    private async Task Initialize()
    {
        //Initialize user
        var user = await _userService.GetUserOrNull(_userInfo);
        if (user == null)
        {
            var addUserTask = _userService.AddUserFromTelegram(_userInfo);
            await ChatIo.SendMessageAsync(_settings.WelcomeMessage);
            user = await addUserTask;
            Reporter.WriteInfo($"New user {user.TelegramNick}", user.TelegramId.ToString());
        }

        Chat = new ChatRoom(ChatIo, user);
        // Initialize update hooks
        _translationSelectedUpdateHook =
            new TranslationSelectedUpdateHook(Chat, _addWordsService, _buttonCallbackDataService);
        _wellKnownWordsUpdateHook = new LeafWellKnownWordsUpdateHook(Chat);

        ChatIo.AddUpdateHook(_translationSelectedUpdateHook);
        ChatIo.AddUpdateHook(_wellKnownWordsUpdateHook);

        // Initialize  command handlers
        _addWordCommandHandler =
            new AddBotCommandHandler(_addWordsService, _buttonCallbackDataService, _translationSelectedUpdateHook);

        ChatIo.CommandHandlers = new[]
        {
            HelpBotCommandHandler.Instance,
            new StatsBotCommandHandler(_settings.ExamSettings),
            new LearnBotCommandHandler(
                _userService, _usersWordsService, _questionSelector, _settings.ExamSettings,
                _frequentWordService, _addWordsService, _localDictionaryService),
            _addWordCommandHandler,
            new ShowWellLearnedWordsCommandHandler(_usersWordsService, _wellKnownWordsUpdateHook),
            new StartBotCommandHandler(ShowMainMenu),
            new ChlangBotCommandHandler(_userService),
            ReportBotCommandHandler.Instance,
            new SettingsBotCommandHelper(_userService),
            new RemoveWordCommandHandler(_usersWordsService),
            new AddFrequentWordsCommandHandler(
                _frequentWordService, _userService, _usersWordsService, _addWordsService,
                _localDictionaryService, _questionSelector, _settings.ExamSettings),
            new RepeatGoalStreakNotificationCommandHandler(_userService),
            new SettingsEnableNotifications(_userService),
            
            new InternalMutualCommandHandler(_mutualPhrasesService),
            new InternalStatsUpdateCommandHandler(_userService, _usersWordsService),
            new InternalShowWordsStats(_frequentWordService)
        };
    }

    private ChatRoom Chat { get; set; }

    private async Task HandleMainScenario((string Command, IBotCommandHandler CommandHandler)? command)
    {
        Chat.User.OnAnyActivity();
        if (command.HasValue)
        {
            // if main scenario was interrupted by command - then handle the command
            await command.Value.CommandHandler.Execute(command.Value.Command, Chat);
        }
        else
        {
            // handle user input as "translate" handler
            var message = await ChatIo.WaitUserInputAsync();
            if (message.Message?.Text != null)
                await _addWordCommandHandler.Execute(message.Message?.Text, Chat);
            else
                await ChatIo.SendMessageAsync(Chat.Texts.EnterWordOrStart);
        }
    }

    private Task SendNotAllowedTooltip() => ChatIo.SendTooltip(Chat.Texts.ActionIsNotAllowed);

    private async Task ShowMainMenu()
    {
        while (true)
        {
            var commands = new[]
            {
                new[] { InlineButtons.Translation(Chat.Texts) },
                new[] { InlineButtons.Learn(Chat.Texts) },
                Chat.User.WasInterfaceLanguageChanged ? null : new[] { InlineButtons.Chlang(Chat.Texts) },
                new[] { InlineButtons.Stats(Chat.Texts), InlineButtons.HowToUse(Chat.Texts) },
                new[] { InlineButtons.Settings(Chat.Texts) }
            };

            var isNewUser = !Chat.User.WasInterfaceLanguageChanged && Chat.User.WordsCount == 0 &&
                            Chat.User.EnglishWordTranslationRequestsCount == 0
                            && Chat.User.RussianWordTranslationRequestsCount == 0;

            var message = Markdown.Escaped($"{Emojis.MainMenu}") + (isNewUser
                ? Chat.Texts.MainMenuTextForNewUserText
                : Chat.Texts.MainMenuText);
            await ChatIo.SendMessageAsync(message, commands.Where(c => c != null).ToArray());

            while (true)
            {
                var action = await ChatIo.WaitUserInputAsync();

                if (action.Message != null)
                {
                    await _addWordCommandHandler.Execute(action.Message.Text, Chat);
                    return;
                }

                if (action.CallbackQuery != null)
                {
                    var btn = action.CallbackQuery.Data;
                    // button data contains same commands as in menu items 
                    var commandHandler = ChatIo.CommandHandlers.FirstOrDefault(c => c.Acceptable(btn));
                    if (commandHandler != null)
                    {
                        await commandHandler.Execute(null, Chat);
                        return;
                    }
                }

                await SendNotAllowedTooltip();
            }
        }
    }
}