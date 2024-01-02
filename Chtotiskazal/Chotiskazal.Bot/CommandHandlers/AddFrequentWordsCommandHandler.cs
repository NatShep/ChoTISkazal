using System.Threading.Tasks;
using Chotiskazal.Bot.ChatFlows;
using SayWhat.Bll.Services;

namespace Chotiskazal.Bot.CommandHandlers;

public class AddFrequentWordsCommandHandler : IBotCommandHandler {
    private readonly FrequentWordService _frequentWordService;
    private readonly UserService _userService;
    private readonly UsersWordsService _usersWordsService;
    private readonly AddWordService _addWordService;
    private readonly LocalDictionaryService _localDictionary;

    public AddFrequentWordsCommandHandler(   
        FrequentWordService frequentWordService,
        UserService userService,
        UsersWordsService usersWordsService,
        AddWordService addWordService, 
        LocalDictionaryService localDictionary)
    {
        _frequentWordService = frequentWordService;
        _userService = userService;
        _usersWordsService = usersWordsService;
        _addWordService = addWordService;
        _localDictionary = localDictionary;
    }

    public bool Acceptable(string text) => text == BotCommands.New;
    
    public string ParseArgument(string text) => null;

    public async Task Execute(string argument, ChatRoom chat)
    {
        var frequentWordFlow = new AddWordFromFrequentWordsFlow(
            chat, _frequentWordService, _userService,
            _usersWordsService, _addWordService, _localDictionary);
        await frequentWordFlow.EnterAsync();
    }
}