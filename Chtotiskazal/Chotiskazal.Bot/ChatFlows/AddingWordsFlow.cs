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
                if(!await EnterSingleWordAsync(userId, yaStatus.isYaDicOnline, word))
                    break;
                word = null;
            }
        }

        async Task<bool> EnterSingleWordAsync(int userId, bool isYaDicOnline, string? word = null)
        {
            if (word == null)
            {
                await _chatIo.SendMessage("Enter english word", new InlineKeyboardButton
                {
                    CallbackData = "/exit",
                    Text = "Cancel"
                });
                while (true)
                {
                    var input = await _chatIo.WaitUserInput();
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
            var translations =await _addWordService.FindInDictionaryWithPhrases(word);
            if (!translations.Any() && isYaDicOnline)
                translations =await _addWordService.TranslateAndAddToDictionary(word);
            if (!translations.Any())
            {
                await _chatIo.SendMessage("No translations found. Check the word and try again");
                return true;
            }

            await _chatIo.SendMessage($"Choose translation for '{word}'",
                InlineButtons.CreateVariantsWithCancel(translations.Select(t => t.UserTranslations)));
            
            while (true)
            {
                var input = await _chatIo.TryWaitInlineIntKeyboardInput();
                if (!input.HasValue )
                    return false;
                if (input.Value >= 0 && input.Value < translations.Count)
                {
                    var selected = translations[input.Value];
                    var count =await _addWordService.AddSomeWordsToUserCollectionAsync(userId, new UserWordForLearning[]{selected});
                    await _chatIo.SendMessage($"Saved. Translations: {count}");
                    return true;
                }
            }
        }
    }

    }
