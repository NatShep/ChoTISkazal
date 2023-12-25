using System.Threading.Tasks;
using SayWhat.Bll.Services;

namespace Chotiskazal.Bot.CommandHandlers;

public class InternalStatsUpdateCommandHandler : IBotCommandHandler {
    private readonly UserService _userService;
    private readonly UsersWordsService _usersWordsService;

    public InternalStatsUpdateCommandHandler(UserService userService, UsersWordsService usersWordsService)
    {
        _userService = userService;
        _usersWordsService = usersWordsService;
    }

    public bool Acceptable(string text) => text == "/updatestats";
    
    public string ParseArgument(string text) => null;

    public async Task Execute(string argument, ChatRoom chat)
    {
        await chat.SendTyping();
        var allWords = await _usersWordsService.GetAllWords(chat.User);
        chat.User.RecreateStatistic(allWords);
        await _userService.Update(chat.User);
        await chat.SendMessageAsync("All stats updated");
    }
}