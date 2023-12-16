using System.Threading.Tasks;
using Chotiskazal.Bot.ChatFlows;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll.Services;

namespace Chotiskazal.Bot.CommandHandlers;

public class LearnBotCommandHandler : IBotCommandHandler {
    private readonly UserService _userService;
    private readonly UsersWordsService _usersWordsService;
    private readonly ExamSettings _examSettings;
    private readonly QuestionSelector _questionSelector;

    public LearnBotCommandHandler(
        UserService userService,
        UsersWordsService usersWordsService,
        ExamSettings examSettings, 
        QuestionSelector questionSelector) {
        _userService = userService;
        _usersWordsService = usersWordsService;
        _examSettings = examSettings;
        _questionSelector = questionSelector;
    }

    public bool Acceptable(string text) => text == BotCommands.Learn;
    public string ParseArgument(string text) => null;

    public Task Execute(string argument, ChatRoom chat) => new ExamFlow(
            chat, _userService, _usersWordsService, _examSettings, _questionSelector)
        .EnterAsync();
}