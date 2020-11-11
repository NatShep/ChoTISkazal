#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Chotiskazal.Api.Services;
using Chotiskazal.DAL;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.ChatFlows
{
    class AddingWordsMode
    {
        private readonly Chat _chat;
        private readonly AddWordService _addWordService; 

        public AddingWordsMode(
            Chat chat, 
            AddWordService addWordService
            )
        {
            _chat = chat;
            _addWordService = addWordService;
        }
        public async Task Enter(int userId, string? word = null)
        {
            var yaStatus = _addWordService.PingYandex();

            /*if (_yapiDicClient.IsOnline)
                await _chat.SendMessage("Yandex dic is online");
            else
                await _chat.SendMessage("Yandex dic is offline");

            if (_yapiTransClient.IsOnline)
                await _chat.SendMessage("Yandex trans is online");
            else
                await _chat.SendMessage("Yandex trans is offline");*/

            while (true)
            {
                if(!await EnterSingleWord(userId, yaStatus.isYaDicOnline, word))
                    break;
                word = null;
            }
        }

        async Task<bool> EnterSingleWord(int userId, bool isYaDicOnline, string? word = null)
        {
            if (word == null)
            {
                await _chat.SendMessage("Enter english word", new InlineKeyboardButton
                {
                    CallbackData = "/exit",
                    Text = "Cancel"
                });
                while (true)
                {
                    var input = await _chat.WaitUserInput();
                    if (input.CallbackQuery != null && input.CallbackQuery.Data == "/exit")
                        throw new ProcessInteruptedWithMenuCommand("/start");

                    if (!string.IsNullOrEmpty(input.Message.Text))
                    {
                        word = input.Message.Text;
                        break;
                    }
                }
            }
            
            //find word in local dictionary(if not, find it in Ya dictionary)
            var translations = _addWordService.FindInDictionaryWithPhrases(word);
            if (!translations.Any() && isYaDicOnline)
                translations = _addWordService.TranslateAndAddToDictionary(word);
            if (!translations.Any())
            {
                await _chat.SendMessage("No translations found. Check the word and try again");
                return true;
            }

            await _chat.SendMessage($"Choose translation for '{word}'",
                InlineButtons.CreateVariantsWithCancel(translations.Select(t => t.UserTranslations)));
            
            while (true)
            {
                var input = await _chat.TryWaitInlineIntKeyboardInput();
                if (!input.HasValue )
                    return false;
                if (input.Value >= 0 && input.Value < translations.Count)
                {
                    var selected = translations[input.Value];
                    var count =_addWordService.AddResultToUserCollection(userId, new UserWordForLearning[]{selected});
                    await _chat.SendMessage($"Saved. Translations: {count}");
                    return true;
                }
            }
        }
    }

    }
