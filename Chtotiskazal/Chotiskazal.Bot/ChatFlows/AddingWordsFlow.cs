#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.Bll.Dto;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Users;

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

        public async Task Enter(UserModel user, string? word = null)
        {
            do {
                word = await EnterSingleWordAsync(user, word);
            } while (!string.IsNullOrWhiteSpace(word));
        }

        private async Task<string?> EnterSingleWordAsync(UserModel user,  string? word = null)
        {
            if (word == null)
            {
                await _chatIo.SendMessageAsync("Enter english or russian word to translate or /start to open main menu ");
                word = await _chatIo.WaitUserTextInputAsync();
            }
            user.RegistrateActivity();

            //find word in local dictionary(if not, find it in Ya dictionary)
            var translations = await _addWordService.FindInDictionaryWithExamples(word);
            if (!translations.Any())
                translations = await _addWordService.TranslateAndAddToDictionary(word);
            if (translations?.Any()!=true)
            {
                await _chatIo.SendMessageAsync("No translations found. Check the word and try again");
                return null;
            }

            var tr = translations.FirstOrDefault(t =>!string.IsNullOrWhiteSpace(t.EnTranscription))?.EnTranscription;


            await _chatIo.SendMarkdownMessageAsync(
                $"_Here are the translations\\._ \r\n" +
                $"_Choose one of them to learn them in the future_\r\n\r\n" +
                $"*{word.Capitalize()}*" +
                $"{(tr == null ? "\r\n" : $"\r\n```\r\n[{tr}]\r\n```")}",
                    InlineButtons.CreateVariantsWithCancel(translations.Select(t => t.TranslatedText)));
            
            
            if(word.IsRussian())
                user.IncrementRussianWordTranlationRequestsCount();
            else
                user.IncrementEnglishWordTranlationRequestsCount();
            
            while (true)
            {
                var update = await _chatIo.WaitUserInputAsync();
                var input = update.CallbackQuery?.Data;
                if (input == null)
                {
                    await SaveFirstTranslationBecauseOfCancel(user, translations);
                    // user start to translate next word
                    return update.Message?.Text;
                }
                    
                if(!int.TryParse(input, out var selectedIndex))
                    continue;
                
                if (selectedIndex < 0 || selectedIndex >= translations.Count)
                    continue;
                    
                var selected = GetSelectedTranslation(translations, selectedIndex);
                await _addWordService.AddWordsToUser(user,  selected);
                    
                if(selected.Examples.Count>0)
                    await _chatIo.SendMessageAsync($"Saved. Examples: {selected.Examples.Count}");
                else
                    await _chatIo.SendMessageAsync($"Saved.");
                return null;
            }
        }

        private async Task SaveFirstTranslationBecauseOfCancel(UserModel user, IReadOnlyList<DictionaryTranslation> translations)
        {
            // if user did not select the word before next choose
            // than we automaticly add first translation
            // but we can not be sure that it is new word for user
            // so we give score '8' to that pair, witch means 'familiar word'
            // and user needs at least one or two exams to pass the word
            var selected = GetSelectedTranslation(translations, 0);
            await _addWordService.AddWordsToUser(user, selected, 8);
        }

        private static DictionaryTranslation GetSelectedTranslation(IReadOnlyList<DictionaryTranslation> translations, int value)
        {
            var selected = translations[value];
            if (selected.TranlationDirection == TranlationDirection.RuEn)
                selected = selected.GetReversed(); //Reverse translate direction, to store all pairs in one dirrection
            return selected;
        }
    }
}