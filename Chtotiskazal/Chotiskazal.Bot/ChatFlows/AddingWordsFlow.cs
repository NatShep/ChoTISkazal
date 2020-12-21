#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL;

namespace Chotiskazal.Bot.ChatFlows
{
    internal class AddingWordsMode
    {
        private ChatRoom Chat { get; }
        private readonly AddWordService _addWordService;
        private readonly TranslationSelectedUpdateHook _translationSelectedUpdateHook;

        public AddingWordsMode(
            ChatRoom chat,
            AddWordService addWordService,
            TranslationSelectedUpdateHook translationUpdateHookHandler
            )
        {
            Chat = chat;
            _addWordService = addWordService;
            _translationSelectedUpdateHook = translationUpdateHookHandler;
        }

        public async Task Enter(string? word = null)
        {
            do {
                word = await EnterSingleWordAsync(word);
            } while (!string.IsNullOrWhiteSpace(word));
        }

        private async Task<string?> EnterSingleWordAsync(string? word = null)
        {
            if (word == null)
            {
                await Chat.SendMessageAsync(Chat.Texts.EnterWordOrStart);
                word = await Chat.WaitUserTextInputAsync();
            }

            Chat.User.OnAnyActivity();

            // Search translations in local dictionary
            
            var translations = await _addWordService.FindInDictionaryWithExamples(word);
            if (!translations.Any()) // if not, search it in Ya dictionary
                translations = await _addWordService.TranslateAndAddToDictionary(word);
            
            // Inline keyboards has limitation for size of the message 
            // Workaraound: exclude all translations that are more than 32 symbols
            if(translations!=null && translations.Any(t=>t.OriginText.Length + t.TranslatedText.Length>32))
                translations = translations.Where(t => t.OriginText.Length + t.TranslatedText.Length <= 32).ToArray();

            if (translations?.Any() != true)
            {
                await Chat.SendMessageAsync(Chat.Texts.NoTranslationsFound);
                return null;
            }

            // Automaticly select first translation
            await _addWordService.AddTranslationToUser(Chat.User, translations[0]);
            
            // getting first transcription
            var transcription = translations.FirstOrDefault(t => !string.IsNullOrWhiteSpace(t.EnTranscription))?.EnTranscription;
                        
            var handler = new LastTranslationHandler(
                translations: translations,
                chat: Chat,
                addWordService: _addWordService);
            
            _translationSelectedUpdateHook.SetTranslationHandler(handler);

            await handler.SendTranslationMessage(Chat.Texts.HereAreTheTranslationMarkdown(word, transcription));

            if (word.IsRussian()) Chat.User.OnRussianWordTranlationRequest();
            else Chat.User.OnEnglishWordTranslationRequest();

            try
            {
                // user request next word
                var res = await Chat.WaitUserTextInputAsync();
                // notify handler, that we leave the flow
                return res;
            }
            finally
            {
                handler.OnNextTranslationRequest();
            }
        }
    }
}