#nullable enable
using System;
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
        private readonly TranslationSelectedQueryHandler _translationSelectedQueryHandler;

        public AddingWordsMode(
            ChatIO chatIo,
            AddWordService addWordService,
            TranslationSelectedQueryHandler translationQueryHandlerHandler
            )
        {
            _chatIo = chatIo;
            _addWordService = addWordService;
            _translationSelectedQueryHandler = translationQueryHandlerHandler;
        }

        public async Task Enter(UserModel user, string? word = null)
        {
            do {
                word = await EnterSingleWordAsync(user, word);
            } while (!string.IsNullOrWhiteSpace(word));
        }

        private async Task<string?> EnterSingleWordAsync(UserModel user, string? word = null)
        {
            if (word == null)
            {
                await _chatIo.SendMessageAsync(
                    "Enter english or russian word to translate or /start to open main menu ");
                word = await _chatIo.WaitUserTextInputAsync();
            }

            user.OnAnyActivity();

            // Search translations in local dictionary
            var translations = await _addWordService.FindInDictionaryWithExamples(word);
            if (!translations.Any()) // if not, find it in Ya dictionary
                translations = await _addWordService.TranslateAndAddToDictionary(word);
            if (translations?.Any() != true)
            {
                await _chatIo.SendMessageAsync("No translations found. Check the word and try again");
                return null;
            }

            // getting first transcription
            var tr = translations.FirstOrDefault(t => !string.IsNullOrWhiteSpace(t.EnTranscription))?.EnTranscription;
                        
            var handler = new LastTranslationHandler(
                translations: translations,
                user: user,
                chat: _chatIo,
                addWordService: _addWordService);

            _translationSelectedQueryHandler.SetTranslationHandler(handler);

            var messageId = await _chatIo.SendMarkdownMessageAsync(
                $"_Here are the translations\\._ \r\n" +
                $"_Choose one of them to learn them in the future_\r\n\r\n" +
                $"*{word.Capitalize()}*" +
                $"{(tr == null ? "\r\n" : $"\r\n```\r\n[{tr}]\r\n```")}",
                translations.Select(v => AddWordHelper.CreateButtonFor(v, false)).ToArray());

            handler.SetMessageId(messageId);

            if (word.IsRussian()) user.OnRussianWordTranlationRequest();
            else user.OnEnglishWordTranslationRequest();

            try
            {
                // user request next word
                var res = await _chatIo.WaitUserTextInputAsync();
                // notify handler, that we leave the flow
                // If user does not choose any translation for X seconds
                // first option gonna be choosen automaticly
                handler.OnNextTranslationRequest();
                return res;
            }
            catch (Exception)
            {
                handler.OnFlowInterrupted();
                throw;
            }
        }
    }
}