using System;
using System.Threading.Tasks;
using Chotiskazal.Bot.ChatFlows;
using Chotiskazal.Bot.Hooks;
using SayWhat.Bll;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot {

public interface IBotCommandHandler {
    bool Acceptable(string text);
    string ParseArgument(string text);
    Task Execute(string argument, ChatRoom chat);
}

public class HelpBotCommandHandler : IBotCommandHandler {
    public static IBotCommandHandler Instance { get; } = new HelpBotCommandHandler();
    private HelpBotCommandHandler() { }
    public bool Acceptable(string text) => text == BotCommands.Help;
    public string ParseArgument(string text) => null;

    public Task Execute(string argument, ChatRoom chat) => chat.SendMarkdownMessageAsync(
        chat.Texts.HelpMarkdown,
        new[] {
            new[] {
                InlineButtons.MainMenu($"{Emojis.MainMenu} {chat.Texts.MainMenuButton}")
            }
        });
}

public class AddBotCommandHandler : IBotCommandHandler {
    private readonly AddWordService _addWordsService;
    private readonly TranslationSelectedUpdateHook _translationSelectedUpdateHook;

    public AddBotCommandHandler(
        AddWordService addWordsService, TranslationSelectedUpdateHook translationSelectedUpdateHook) {
        _addWordsService = addWordsService;
        _translationSelectedUpdateHook = translationSelectedUpdateHook;
    }

    public bool Acceptable(string text) => text == BotCommands.Add;
    public string ParseArgument(string text) => null;

    public Task Execute(string argument, ChatRoom chat) =>
        new TranslateWordsFlow(chat, _addWordsService, _translationSelectedUpdateHook).Enter(argument);
}


public class NewBotCommandHandler : IBotCommandHandler {
    private readonly LocalDictionaryService _localDictionaryService;
    private readonly LearningSetService _learningSetService;
    private readonly UserService _userService;
    private readonly UsersWordsService _usersWordsService;
    private readonly AddWordService _addWordsService;

    public NewBotCommandHandler(
        LocalDictionaryService localDictionaryService, LearningSetService learningSetService, UserService userService,
        UsersWordsService usersWordsService, AddWordService addWordsService) {
        _localDictionaryService = localDictionaryService;
        _learningSetService = learningSetService;
        _userService = userService;
        _usersWordsService = usersWordsService;
        _addWordsService = addWordsService;
    }

    public bool Acceptable(string text) => text == BotCommands.New;
    public string ParseArgument(string text) => null;

    public Task Execute(string argument, ChatRoom chat) => new SelectLearningSetsFlow(
            chat,
            _localDictionaryService,
            _learningSetService,
            _userService,
            _usersWordsService,
            _addWordsService)
        .EnterAsync();
}

public class LearnBotCommandHandler : IBotCommandHandler {
    private readonly UserService _userService;
    private readonly UsersWordsService _usersWordsService;
    private readonly ExamSettings _examSettings;

    public LearnBotCommandHandler(
        UserService userService,
        UsersWordsService usersWordsService,
        ExamSettings examSettings) {
        _userService = userService;
        _usersWordsService = usersWordsService;
        _examSettings = examSettings;
    }

    public bool Acceptable(string text) => text == BotCommands.Learn;
    public string ParseArgument(string text) => null;

    public Task Execute(string argument, ChatRoom chat) => new ExamFlow(
            chat, _userService, _usersWordsService, _examSettings)
        .EnterAsync();
}

public class StatsBotCommandHandler : IBotCommandHandler {
    public static IBotCommandHandler Instance { get; } = new StatsBotCommandHandler();
    private StatsBotCommandHandler() { }
    public bool Acceptable(string text) => text == BotCommands.Stats;
    public string ParseArgument(string text) => null;

    public Task Execute(string argument, ChatRoom chat) =>
        chat.SendMarkdownMessageAsync(
            StatsRenderer.GetStatsText(chat).EscapeForMarkdown(),
            new[] {
                new[] {
                    InlineButtons.MainMenu($"{Emojis.MainMenu} {chat.Texts.MainMenuButton}"),
                    InlineButtons.Exam($"{chat.Texts.LearnButton} {Emojis.Learning}"),
                },
                new[] { InlineButtons.Translation($"{chat.Texts.TranslateButton} {Emojis.Translate}") },
                new[] {
                    InlineButtons.WellLearnedWords(
                        $"{chat.Texts.ShowWellKnownWords} ({chat.User.CountOf((int)WordLeaningGlobalSettings.LearnedWordMinScore, 10)}) {Emojis.SoftMark}")
                }
            });
}

public class StartBotCommandHandler : IBotCommandHandler {
    private readonly Func<Task> _showMainMenu;
    public StartBotCommandHandler(Func<Task> showMainMenu) { _showMainMenu = showMainMenu; }

    public bool Acceptable(string text) => text == BotCommands.Start;
    public string ParseArgument(string text) => null;

    public Task Execute(string argument, ChatRoom chat) => _showMainMenu();
}

}