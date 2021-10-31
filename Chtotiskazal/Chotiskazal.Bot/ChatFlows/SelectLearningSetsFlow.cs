using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.WordKits;

namespace Chotiskazal.Bot.ChatFlows {

public class SelectLearningSetsFlow {
    private readonly LocalDictionaryService _localDictionaryService;
    private readonly LearningSetService _learningSetService;
    private readonly UsersWordsService _usersWordsService;
    private readonly AddWordService _addWordService;
    private readonly UserService _userService;

    public SelectLearningSetsFlow(
        ChatRoom chat,
        LocalDictionaryService localDictionaryService,
        LearningSetService learningSetService,
        UserService userService,
        UsersWordsService usersWordsService,
        AddWordService addWordService) {
        _localDictionaryService = localDictionaryService;
        _learningSetService = learningSetService;
        _usersWordsService = usersWordsService;
        _addWordService = addWordService;
        _userService = userService;
        Chat = chat;
    }

    private ChatRoom Chat { get; }

    public async Task EnterAsync() {
        var allSets = await _learningSetService.GetAllSets();

        var msg = new StringBuilder("Some sets:\r\n");
        foreach (var learningSet in allSets)
        {
            msg.AppendLine(
                "/set_" + learningSet.ShortName + "   " + learningSet.EnName + "\r\n" + learningSet.EnDescription);
            msg.AppendLine();
        }

        //_learningSetSelector.Set(await _learningSetService.GetAllSets());
        await Chat.SendMessageAsync(
            msg.ToString(), InlineButtons.MainMenu($"{Emojis.MainMenu} {Chat.Texts.MainMenuButton}"));

        LearningSet set = null;
        while (true)
        {
            var input = await Chat.WaitUserTextInputAsync();

            if (!input.StartsWith("/set_"))
            {
                await Chat.SendMessageAsync("Choose set to learn");
                continue;
            }

            var setName = input.Substring(5).Trim();
            set = allSets.FirstOrDefault(s => s.ShortName.Equals(setName, StringComparison.InvariantCultureIgnoreCase));
            if (set == null)
            {
                await Chat.SendMessageAsync($"Set {setName} is not found");
                continue;
            }
            else
            {
                break;
            }
        }
        var addFlow = new AddFromLearningSetFlow(
            chat: Chat, 
            localDictionaryService: _localDictionaryService, 
            set: set, 
            userService: _userService,
            usersWordsService: _usersWordsService, 
            addWordService: _addWordService);
        await addFlow.EnterAsync();
    }
}

}