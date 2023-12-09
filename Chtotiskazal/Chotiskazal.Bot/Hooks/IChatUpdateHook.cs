using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Chotiskazal.Bot.Hooks;

/// <summary>
/// Hi priority inline query handler
/// </summary>
public interface IChatUpdateHook
{
    /// <summary>
    /// Check if the update can be handled by hook
    /// </summary>
    bool CanBeHandled(Update update);
    /// <summary>
    /// Handle update. the update is intercepted, so chat flow not handle it anymore
    /// </summary>
    Task Handle(Update update);
}