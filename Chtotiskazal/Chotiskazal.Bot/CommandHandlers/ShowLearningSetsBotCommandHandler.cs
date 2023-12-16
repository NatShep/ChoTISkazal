using System.Text;
using System.Threading.Tasks;
using SayWhat.Bll.Services;

namespace Chotiskazal.Bot.CommandHandlers;

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
        foreach (var learningSet in allSets) {
            msg.AppendLine(
                chat.User.IsEnglishInterface
                    ? $"{BotCommands.LearningSetPrefix}_{learningSet.ShortName}   {learningSet.EnName}\r\n{learningSet.EnDescription}\r\n"
                    : $"{BotCommands.LearningSetPrefix}_{learningSet.ShortName}   {learningSet.RuName}\r\n{learningSet.RuDescription}\r\n");
        }

        await chat.SendMessageAsync(msg.ToString());
    }
}