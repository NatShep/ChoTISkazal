#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.Bll.Dto;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Users;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.ChatFlows
{
    internal class AddingWordsMode
    {
        private readonly ChatIO _chatIo;
        private readonly AddWordService _addWordService;
        private readonly SelectWordTranslationCallbackQueryHandler _translationCallbackQueryHandler;

        public AddingWordsMode(
            ChatIO chatIo,
            AddWordService addWordService,
            SelectWordTranslationCallbackQueryHandler translationCallbackQueryHandler
            )
        {
            _chatIo = chatIo;
            _addWordService = addWordService;
            _translationCallbackQueryHandler = translationCallbackQueryHandler;
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
            user.OnAnyActivity();

            //find word in local dictionary(if not, find it in Ya dictionary)
            var translations = await _addWordService.FindInDictionaryWithExamples(word);
            if (!translations.Any())
                translations = await _addWordService.TranslateAndAddToDictionary(word);
            if (translations?.Any()!=true)
            {
                await _chatIo.SendMessageAsync("No translations found. Check the word and try again");
                return null;
            }
            //getting first transcription
            var tr = translations.FirstOrDefault(t =>!string.IsNullOrWhiteSpace(t.EnTranscription))?.EnTranscription;

            var handler = new ConcreteTranslationFastHandler(
                translations:   translations,
                user:           user,
                chat:           _chatIo, 
                addWordService: _addWordService);
            
            _translationCallbackQueryHandler.SetTranslationHandler(handler);
            
            await _chatIo.SendMarkdownMessageAsync(
                $"_Here are the translations\\._ \r\n" +
                $"_Choose one of them to learn them in the future_\r\n\r\n" +
                $"*{word.Capitalize()}*" +
                $"{(tr == null ? "\r\n" : $"\r\n```\r\n[{tr}]\r\n```")}",
                translations.Select(v => AddWordHelper.CreateButtonFor(v, false))
                    .Append(new InlineKeyboardButton
                    {
                        CallbackData = "/start",
                        Text = "Cancel",
                    }).ToArray());
            
            
            if(word.IsRussian())
                user.OnRussianWordTranlationRequest();
            else
                user.OnEnglishWordTranslationRequest();

            try
            {
                // user start to translate next word
                return await _chatIo.WaitUserTextInputAsync();
            }
            finally
            {
                //notify handler, that we leave the flow
                await handler.OnNextUserMessage();
            }

            /*
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
                    
                var selected = translations[selectedIndex].GetEnRu();
                await _addWordService.AddTranslationToUser(user,  selected);
                    
                if(selected.Examples.Count>0)
                    await _chatIo.SendMessageAsync($"Saved. Examples: {selected.Examples.Count}");
                else
                    await _chatIo.SendMessageAsync($"Saved.");
                return null;*/
            //}
        }

        private async Task SaveFirstTranslationBecauseOfCancel(UserModel user, IReadOnlyList<DictionaryTranslation> translations)
        {
            // if user did not select the word before next choose
            // than we automaticly add first translation
            // but we can not be sure that it is new word for user
            // so we give score '8' to that pair, witch means 'familiar word'
            // and user needs at least one or two exams to pass the word
            var selected = translations[0].GetEnRu();
            await _addWordService.AddTranslationToUser(user, selected, 8);
        }
    }
}