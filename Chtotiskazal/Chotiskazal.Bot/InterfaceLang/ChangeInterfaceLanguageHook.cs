using System.Threading.Tasks;
using Chotiskazal.Bot.Hooks;
using Chotiskazal.Bot.InterfaceLang;
using Telegram.Bot.Types;

namespace Chotiskazal.Bot
{
    public class ChangeInterfaceLanguageHook: IChatUpdateHook
    {
        private readonly ChatIO _chatIo;

        public ChangeInterfaceLanguageHook(ChatIO chatIo)
        {
            _chatIo = chatIo;
        }
        public IInterfaceTexts SelectedInterfaceLanguage { get; private set; } = new EnglishTexts();
        public bool CanBeHandled(Update update)
        {
            var text = update.Message?.Text;
            return text == "/ru" || text == "/en";
        }

        public Task Handle(Update update)
        {
            var text = update.Message?.Text;
            if (text == "/en")
            {
                SelectedInterfaceLanguage = new EnglishTexts();
            }
            else
            {
                SelectedInterfaceLanguage = new RussianTexts();
            }

            return _chatIo.SendMessageAsync(SelectedInterfaceLanguage.InterfaceLanguageSetuped);
        }
    }
}