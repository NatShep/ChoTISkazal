using System;
using System.Threading.Tasks;

namespace Chotiskazal.Bot.CommandHandlers;

public class StartBotCommandHandler : IBotCommandHandler {
    private readonly Func<Task> _showMainMenu;

    public StartBotCommandHandler(Func<Task> showMainMenu) {
        _showMainMenu = showMainMenu;
    }

    public bool Acceptable(string text) => text == BotCommands.Start;
    public string ParseArgument(string text) => null;

    public Task Execute(string argument, ChatRoom chat) => _showMainMenu();
}