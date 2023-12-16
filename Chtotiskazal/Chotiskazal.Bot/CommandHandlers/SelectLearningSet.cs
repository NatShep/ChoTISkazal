using System;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.ChatFlows;
using SayWhat.Bll.Services;

namespace Chotiskazal.Bot.CommandHandlers;

public class SelectLearningSet : IBotCommandHandler {
    private const string Prefix = BotCommands.LearningSetPrefix + "_";
    private readonly LearningSetService _learningSetService;
    private readonly LocalDictionaryService _localDictionaryService;
    private readonly UserService _userService;
    private readonly UsersWordsService _usersWordsService;
    private readonly AddWordService _addWordService;

    public SelectLearningSet(LearningSetService learningSetService, LocalDictionaryService localDictionaryService,
        UserService userService, UsersWordsService usersWordsService, AddWordService addWordService) {
        _learningSetService = learningSetService;
        _localDictionaryService = localDictionaryService;
        _userService = userService;
        _usersWordsService = usersWordsService;
        _addWordService = addWordService;
    }

    public bool Acceptable(string text) => text?.StartsWith(Prefix) == true;

    public string ParseArgument(string text) => text[Prefix.Length..].Trim();

    public async Task Execute(string argument, ChatRoom chat) {
        var allSets = await _learningSetService.GetAllSets();
        var set = allSets.FirstOrDefault(s =>
            s.ShortName.Equals(argument, StringComparison.InvariantCultureIgnoreCase));
        if (set == null) {
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