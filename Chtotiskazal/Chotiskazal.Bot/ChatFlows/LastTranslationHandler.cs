using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.Bll.Dto;
using SayWhat.Bll.Services;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.ChatFlows
{
    public class LastTranslationHandler {
        private ChatRoom Chat { get; }
        private readonly AddWordService _addWordService;
        private readonly IReadOnlyList<Translation> _translations;
        private bool _isLastMessageInTheChat =true ;

        public string OriginWordText { get;  }
        
        private bool[] _areSelected;
        
        public LastTranslationHandler(
            IReadOnlyList<Translation> translations, 
            ChatRoom chat,
            AddWordService addWordService)
        {
            Chat = chat;
            OriginWordText = translations[0].OriginText;
            _translations = translations;
            _areSelected = new bool[_translations.Count];
            if(translations.Count>0)
                _areSelected[0] = true;
            _addWordService = addWordService;
        }

        public async Task Handle(string translation, Update update)
        {
            var index = AddWordHelper.FindIndexOf(_translations, translation);
            if (index == -1)
                return;
            
            await SelectIthTranslation(update.CallbackQuery.Message.MessageId, index);
            var message = "";
            
            if (_areSelected[index])
                message = Chat.Texts.MessageAfterTranslationIsSelected(_translations[index]);
            else
                message = Chat.Texts.MessageAfterTranslationIsDeselected(_translations[index]);


            await Chat.AnswerCallbackQueryWithTooltip(update.CallbackQuery.Id, message);
        }

        private async Task SelectIthTranslation(int messageId, int index)
        {
            _areSelected[index] = !_areSelected[index];

            if (_areSelected[index]) //if become selected
            {
                await _addWordService.AddTranslationToUser(Chat.User, _translations[index].GetEnRu());
                Reporter.ReportTranslationPairSelected(Chat.User.TelegramId);
            }
            else {
                await _addWordService.RemoveTranslationFromUser(Chat.User, _translations[index].GetEnRu());
                Reporter.ReportTranslationPairRemoved(Chat.User.TelegramId);
            }

            var buttons = CreateButtons();

            await Chat.EditMessageButtons(messageId,buttons.ToArray());
        }

        private IList<InlineKeyboardButton[]> CreateButtons() {
            var buttons = new List<InlineKeyboardButton[]>();
            var i = 0;
            foreach (var translation in _translations) {
                var button = AddWordHelper.CreateButtonFor(translation, _areSelected[i]);
                if (button.CallbackData.Length >= InlineButtons.MaxDataStringLength) 
                {
                    Reporter.ReportError(Chat.ChatId.Identifier, $"Too long button data: '{button.Text}':'{button.CallbackData}'");
                }
                else 
                {
                    buttons.Add(new[]{button});
                }
                i++;
            }
            if (_isLastMessageInTheChat)
                buttons.Add(new[]
                {
                    InlineButtons.MainMenu($"{Emojis.MainMenu} {Chat.Texts.MainMenuButton}"), 
                    InlineButtons.Translation($"{Chat.Texts.ContinueTranslateButton} {Emojis.Translate}")
                });
            return buttons;
        }

        /// <summary>
        /// User make next request
        /// </summary>
        public void OnNextTranslationRequest()
        {
            _isLastMessageInTheChat = false;
            if (_originMessageId!=null)
            {
                //hide "menu" and "translation" buttons
                var buttons = CreateButtons();
                var _ = Chat.EditMessageButtons(_originMessageId.Value, buttons.ToArray());
            }
        }
        /// <summary>
        /// Origin message of translation
        /// </summary>
        private int? _originMessageId;
        public async Task SendTranslationMessage(string markdownMessage, string transcription, bool[] selectionMarks)
        {
            _areSelected = selectionMarks;
            _originMessageId = await Chat.SendMarkdownMessageAsync(
                Chat.Texts.HereAreTheTranslation(markdownMessage, transcription), 
                CreateButtons().ToArray());
        }
    }
}