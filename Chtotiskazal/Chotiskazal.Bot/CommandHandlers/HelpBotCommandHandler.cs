using System.Threading.Tasks;

namespace Chotiskazal.Bot.CommandHandlers;

public class HelpBotCommandHandler : IBotCommandHandler {
    public static IBotCommandHandler Instance { get; } = new HelpBotCommandHandler();
    private HelpBotCommandHandler() { }
    public bool Acceptable(string text) => text == BotCommands.Help;
    public string ParseArgument(string text) => null;

    public Task Execute(string argument, ChatRoom chat) => chat.SendMarkdownMessageAsync(
        chat.Texts.Help, InlineButtons.MainMenu(chat.Texts)
    );
}