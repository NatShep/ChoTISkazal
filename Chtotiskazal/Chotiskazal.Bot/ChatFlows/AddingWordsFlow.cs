#nullable enable
using System.Linq;
using System.Threading.Tasks;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Users;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.ChatFlows
{
    internal class AddingWordsMode
    {
        private readonly ChatIO _chatIo;
        private readonly AddWordService _addWordService;

        public AddingWordsMode(
            ChatIO chatIo,
            AddWordService addWordService
            )
        {
            _chatIo = chatIo;
            _addWordService = addWordService;
        }

        public async Task Enter(User user, string? word = null)
        {
            while (true)
            {
                if (!await EnterSingleWordAsync(user,  word))
                    break;
                word = null;
            }
        }

        private async Task<bool> EnterSingleWordAsync(User user,  string? word = null)
        {
            if (word == null)
            {
                await _chatIo.SendMessageAsync("Enter english word", new InlineKeyboardButton
                {
                    CallbackData = "/exit",
                    Text = "Cancel"
                });
                while (true)
                {
                    var input = await _chatIo.WaitUserInputAsync();
                    if (input.CallbackQuery != null && input.CallbackQuery.Data == "/exit")
                        throw new ProcessInterruptedWithMenuCommand("/start");

                    if (!string.IsNullOrEmpty(input.Message.Text))
                    {
                        word = input.Message.Text;
                        break;
                    }
                }
            }

            //find word in local dictionary(if not, find it in Ya dictionary)
            var translations = await _addWordService.FindInDictionaryWithPhrases(word);
            if (!translations.Any())
                translations = await _addWordService.TranslateAndAddToDictionary(word);
            if (translations?.Any()!=true)
            {
                await _chatIo.SendMessageAsync("No translations found. Check the word and try again");
                return true;
            }

            await _chatIo.SendMessageAsync($"Choose translation for '{word}'",
                InlineButtons.CreateVariantsWithCancel(translations.Select(t => t.RuWord)));

            while (true)
            {
                var input = await _chatIo.TryWaitInlineIntKeyboardInputAsync();
                if (!input.HasValue)
                    return false;
                if (input!.Value < 0 || input.Value >= translations.Count)
                    continue;
                
                var selected = translations[input.Value];
                var count = await _addWordService.AddSomeWordsToUserCollectionAsync(user, new[] {selected});
                await _chatIo.SendMessageAsync($"Saved. Translations: {count}");
                return true;
            }
        }
    }
}