using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Chotiskazal.Bot.ChatFlows
{
    /// <summary>
    /// Hi priority inline query handler
    /// </summary>
    public interface IChatQueryHandler
    {
        /// <summary>
        /// Check if the query can be handled by handler
        /// </summary>
        bool CanBeHandled( CallbackQuery query);
        /// <summary>
        /// Handle update. the update is intercepted, so chat flow not handle it anymore
        /// </summary>
        Task Handle(Update update);
    }
}