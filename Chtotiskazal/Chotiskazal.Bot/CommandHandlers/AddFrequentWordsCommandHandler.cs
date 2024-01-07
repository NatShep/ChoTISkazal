using System.Threading.Tasks;
using Chotiskazal.Bot.ChatFlows.FlowLearning;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll.Services;

namespace Chotiskazal.Bot.CommandHandlers;

public class AddFrequentWordsCommandHandler : IBotCommandHandler
{
    private readonly FrequentWordService _frequentWordService;
    private readonly UserService _userService;
    private readonly UsersWordsService _usersWordsService;
    private readonly AddWordService _addWordService;
    private readonly LocalDictionaryService _localDictionary;
    private readonly ExamSettings _examSettings;
    private readonly QuestionSelector _questionSelector;

    public AddFrequentWordsCommandHandler(
        FrequentWordService frequentWordService,
        UserService userService,
        UsersWordsService usersWordsService,
        AddWordService addWordService,
        LocalDictionaryService localDictionary,
        QuestionSelector questionSelector,
        ExamSettings examSettings)
    {
        _frequentWordService = frequentWordService;
        _userService = userService;
        _usersWordsService = usersWordsService;
        _addWordService = addWordService;
        _localDictionary = localDictionary;
        _questionSelector = questionSelector;
        _examSettings = examSettings;
    }

    public bool Acceptable(string text) => text == BotCommands.AddNewWords;

    public string ParseArgument(string text) => null;

    public Task Execute(string argument, ChatRoom chat) => new NewWordsFlow(chat,
        _frequentWordService, _userService, _usersWordsService, _addWordService,
        _localDictionary, _questionSelector, _examSettings,
        minWordsSelection: 5,
        maxWordSelection: 10,
        preferedQuestionSize: 20).EnterAsync();
}