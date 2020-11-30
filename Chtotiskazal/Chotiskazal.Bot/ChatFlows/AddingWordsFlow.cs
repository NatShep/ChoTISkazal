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
                    CallbackData = "/start",
                    Text = "Cancel"
                });
                while (true)
                {
                    var input = await _chatIo.WaitUserInputAsync();
                    if (input.CallbackQuery != null && input.CallbackQuery.Data == "/start")
                        throw new ProcessInterruptedWithMenuCommand("/start");

                    if (!string.IsNullOrEmpty(input.Message?.Text))
                    {
                        word = input.Message.Text;
                        break;
                    }
                }
            }

            //find word in local dictionary(if not, find it in Ya dictionary)
            var translations = await _addWordService.FindInDictionaryWithoutExamples(word);
            if (!translations.Any())
                translations = await _addWordService.TranslateAndAddToDictionary(word);
            if (translations?.Any()!=true)
            {
                await _chatIo.SendMessageAsync("No translations found. Check the word and try again");
                return true;
            }

            await _chatIo.SendMessageAsync($"Choose translation for '{word}'",
                InlineButtons.CreateVariantsWithCancel(translations.Select(t => t.RuWord)));
            await _addWordService.RegistrateTranslationRequest(user);
            while (true)
            {
                var input = await _chatIo.TryWaitInlineIntKeyboardInput();
                if (!input.HasValue)
                    return false;
                if (input!.Value < 0 || input.Value >= translations.Count)
                    continue;
                
                var selected = translations[input.Value];
                await _addWordService.AddWordsToUser(user, new[] {selected});
                if(selected.Examples.Count>0)
                    await _chatIo.SendMessageAsync($"Saved. Examples: {selected.Examples.Count}");
                else
                    await _chatIo.SendMessageAsync($"Saved.");
                return true;
            }
        }
    }
}