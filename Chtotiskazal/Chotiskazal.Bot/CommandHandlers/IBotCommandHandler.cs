using System.Threading.Tasks;

namespace Chotiskazal.Bot.CommandHandlers;

public interface IBotCommandHandler {
    bool Acceptable(string text);
    string ParseArgument(string text);
    Task Execute(string argument, ChatRoom chat);
}