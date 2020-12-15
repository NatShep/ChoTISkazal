using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Chotiskazal.Bot.ChatFlows
{
    public interface IChatCallbackQuery
    {
        bool CanBeHandled( CallbackQuery query);
        Task Handle(Update update);
    }
}