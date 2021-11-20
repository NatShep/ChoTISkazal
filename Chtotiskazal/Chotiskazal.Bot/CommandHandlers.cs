using System;
using System.Linq;
using System.Text;
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

public class ShowLearningSetsBotCommandHandler : IBotCommandHandler {
    private readonly LearningSetService _learningSetService;

    public ShowLearningSetsBotCommandHandler(LearningSetService learningSetService) {
        _learningSetService = learningSetService;
    }

    public bool Acceptable(string text) => text == BotCommands.New;
    public string ParseArgument(string text) => null;

    public async Task Execute(string argument, ChatRoom chat) {
        var allSets = await _learningSetService.GetAllSets();
        var msg = new StringBuilder($"{chat.Texts.ChooseLearningSet}:\r\n");
        foreach (var learningSet in allSets)
        {
            msg.AppendLine(
                $"{BotCommands.LearningSetPrefix}_" 
                + learningSet.ShortName + "   " 
                + learningSet.EnName + "\r\n" 
                + learningSet.EnDescription+"\r\n");
        }
        await chat.SendMessageAsync(msg.ToString());
    }
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
public class ShowWellLearnedWordsCommandHandler : IBotCommandHandler {
    private UsersWordsService _userWordsService;
    private LeafWellKnownWordsUpdateHook _wellKnownWordsUpdateHook;
    public ShowWellLearnedWordsCommandHandler(UsersWordsService userWordsService, LeafWellKnownWordsUpdateHook wellKnownWordsUpdateHook) {
        _userWordsService = userWordsService;
        _wellKnownWordsUpdateHook = wellKnownWordsUpdateHook;
    }
    public bool Acceptable(string text) => text == BotCommands.Words;
    public string ParseArgument(string text) => null;
    public Task Execute(string argument, ChatRoom chat) => new ShowWellKnownWordsFlow(
            chat, _userWordsService, _wellKnownWordsUpdateHook)
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

public class SelectLearningSet : IBotCommandHandler {
    private const string Prefix = BotCommands.LearningSetPrefix + "_";
    private readonly LearningSetService _learningSetService;
    private readonly LocalDictionaryService _localDictionaryService;
    private readonly UserService _userService;
    private readonly UsersWordsService _usersWordsService;
    private readonly AddWordService _addWordService;
    public SelectLearningSet(LearningSetService learningSetService, LocalDictionaryService localDictionaryService, UserService userService, UsersWordsService usersWordsService, AddWordService addWordService) {
        _learningSetService = learningSetService;
        _localDictionaryService = localDictionaryService;
        _userService = userService;
        _usersWordsService = usersWordsService;
        _addWordService = addWordService;
    }

    public bool Acceptable(string text) => text.StartsWith(Prefix);
    public string ParseArgument(string text) => text[Prefix.Length..].Trim();

    public async Task Execute(string argument, ChatRoom chat) {
        var allSets = await _learningSetService.GetAllSets();
        var set = allSets.FirstOrDefault(s => s.ShortName.Equals(argument, StringComparison.InvariantCultureIgnoreCase));
        if (set == null)
        {
            await chat.SendMessageAsync(chat.Texts.LearningSetNotFound(argument));
            return;
        }
        
        await new AddFromLearningSetFlow(
            chat: chat, 
            localDictionaryService: _localDictionaryService, 
            set: set, 
            userService: _userService,
            usersWordsService: _usersWordsService, 
            addWordService: _addWordService).EnterAsync();
    }
}

}