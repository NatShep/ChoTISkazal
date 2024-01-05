using System.Threading.Tasks;
using Chotiskazal.Bot.ChatFlows.FlowLearning;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll.Services;

namespace Chotiskazal.Bot.CommandHandlers;

public class LearnBotCommandHandler : IBotCommandHandler {
    private readonly ExamSettings _regularExamSettings;
    private readonly QuestionSelector _questionSelector;
    private readonly FrequentWordService _frequentWordService;
    private readonly UserService _userService;
    private readonly UsersWordsService _usersWordsService;
    private readonly AddWordService _addWordService;
    private readonly LocalDictionaryService _localDictionaryService;
    
    public LearnBotCommandHandler(
        UserService userService,
        UsersWordsService usersWordsService,
        QuestionSelector questionSelector, 
        ExamSettings regularExamSettings, 
        FrequentWordService frequentWordService, 
        AddWordService addWordService, 
        LocalDictionaryService localDictionaryService) {
        _userService = userService;
        _usersWordsService = usersWordsService;
        _questionSelector = questionSelector;
        _regularExamSettings = regularExamSettings;
        _frequentWordService = frequentWordService;
        _addWordService = addWordService;
        _localDictionaryService = localDictionaryService;
    }

    public bool Acceptable(string text) => text == BotCommands.Learn;

    public string ParseArgument(string text) => null;

    public Task Execute(string argument, ChatRoom chat) => new LearningFlow(
            chat, 
            _userService,
            _usersWordsService,
            _regularExamSettings,
            _questionSelector,
            _frequentWordService,
            _addWordService,
            _localDictionaryService)
        .EnterAsync();
}