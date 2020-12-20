using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.InterfaceLang;
using SayWhat.Bll.Dto;
using SayWhat.Bll.Services;
using Telegram.Bot.Types;

namespace Chotiskazal.Bot.ChatFlows
{
    public class LastTranslationHandler
    {
        private ChatRoom Chat { get; }
        private readonly AddWordService _addWordService;
        private int _selectedTranslationsCount = 0;
        private readonly IReadOnlyList<DictionaryTranslation> _translations;
        private bool _isLastMessageInTheChat =true ;

        public string OriginWordText { get;  }
        
        private readonly bool[] _areSelected;
        
        public LastTranslationHandler(
            IReadOnlyList<DictionaryTranslation> translations, 
            ChatRoom chat,
            AddWordService addWordService)
        {
            Chat = chat;
            OriginWordText = translations[0].OriginText;
            _translations = translations;
            _areSelected = new bool[_translations.Count];
            _addWordService = addWordService;
        }

        public async Task Handle(string translation, Update update)
        {
            var index = AddWordHelper.FindIndexOf(_translations, translation);
            if (index == -1)
                return;
            if (_areSelected[index])
                return;

            await SelectIthTranslation(update.CallbackQuery.Message.MessageId, index, 0);
            await Chat.AnswerCallbackQueryWithTooltip(update.CallbackQuery.Id,
                Chat.Texts.MessageAfterTranslationIsSelected(_translations[index]));
            if (!_isLastMessageInTheChat)
                return;

            if (_confirmationMessageId.HasValue) {
                if (await Chat.EditMessageText(_confirmationMessageId.Value,
                    Chat.Texts.MessageAfterTranslationIsSelected(_translations[index])))
                    return;
            }

            _confirmationMessageId = await Chat.SendMessageAsync(
                Chat.Texts.MessageAfterTranslationIsSelected(_translations[index]));
        }

        private int? _confirmationMessageId = null;
        private async Task SelectIthTranslation(int messageId, int index, double score)
        {
            _areSelected[index] = true;
            _selectedTranslationsCount++;
            await _addWordService.AddTranslationToUser(Chat.User, _translations[index].GetEnRu(), score);
            await Chat.EditMessageButtons(
                messageId,
                _translations.Select((t, i) => AddWordHelper.CreateButtonFor(t, _areSelected[i])).ToArray()
            );
            
        }
        
        /// <summary>
        /// User requests next translation.
        /// </summary>
        public void OnNextTranslationRequest()
        {
            _isLastMessageInTheChat = false;
            if (_selectedTranslationsCount == 0)
            {
                // run timer that automaticly selects first translation
                // if it is not selected yet
                var _ = RunAutoTranslateTimer();
            }
        }
        private async Task RunAutoTranslateTimer()
        {
            int delay = 10000;
            await Task.Delay(delay);
            if ( _selectedTranslationsCount == 0 && _originMessageId.HasValue)
            {
                // if user did not select the word before for {delay} milliseconds 
                // than we automaticly add first translation
                // but we can not be sure that it is new word for user
                // so we give score '3.7' to that pair, witch means 'familiar word'
                // and user needs at least one or two question to pass the word
                await SelectIthTranslation(
                    messageId: _originMessageId.Value,
                    index: 0,
                    score: 3.7);
            }
        }
        /// <summary>
        /// Origin message of translation
        /// </summary>
        private int? _originMessageId;
        public void SetMessageId(int messageId) => _originMessageId = messageId;
        /// <summary>
        /// Flow was interrupted
        /// </summary>
        public void OnFlowInterrupted()
        {
            _isLastMessageInTheChat = false;
        }
    }
}