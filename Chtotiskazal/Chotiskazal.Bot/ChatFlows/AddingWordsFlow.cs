#nullable enable
using System.Linq;
using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL;
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
                await _chatIo.SendMessageAsync("Enter english or russian word to translate or /start to open main menu ");
                word = await _chatIo.WaitUserTextInputAsync();
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

            var tr = translations.FirstOrDefault(t =>!string.IsNullOrWhiteSpace(t.EnTranscription))?.EnTranscription;


            await _chatIo.SendMarkdownMessageAsync(
                $"_Here are the translations\\._ \r\n" +
                $"_Choose one of them to learn them in the future_\r\n\r\n" +
                $"*{word.Capitalize()}*" +
                $"{(tr == null ? "\r\n" : $"\r\n```\r\n[{tr}]\r\n```")}",
                    InlineButtons.CreateVariantsWithCancel(translations.Select(t => t.TranslatedText)));
            if(word.IsRussian())
                await _addWordService.RegistrateRuTranslationRequest(user);
            else
                await _addWordService.RegistrateEnTranslationRequest(user);
            while (true)
            {
                var input = await _chatIo.TryWaitInlineIntKeyboardInput();
                if (!input.HasValue)
                    return false;
                if (input!.Value < 0 || input.Value >= translations.Count)
                    continue;
                
                var selected = translations[input.Value];
                if (selected.TranlationDirection == TranlationDirection.RuEn)
                    selected = selected.GetReversed(); //Reverse direction
                
                await _addWordService.AddWordsToUser(user,  selected);
                if(selected.Examples.Count>0)
                    await _chatIo.SendMessageAsync($"Saved. Examples: {selected.Examples.Count}");
                else
                    await _chatIo.SendMessageAsync($"Saved.");
                return true;
            }
        }
    }
}