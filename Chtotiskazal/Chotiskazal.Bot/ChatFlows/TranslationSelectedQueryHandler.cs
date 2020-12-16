using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SayWhat.Bll.Dto;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Users;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.ChatFlows
{
    public class TranslationSelectedQueryHandler : IChatQueryHandler
    {
        private readonly AddWordService _addWordService;
        private readonly ChatIO _chat;
        private readonly UserModel _user;
        
        public TranslationSelectedQueryHandler(
            AddWordService addWordService, 
            ChatIO chat, 
            UserModel user)
        {
            _addWordService = addWordService;
            _chat = chat;
            _user = user;
        }

        public void SetTranslationHandler(LastTranslationHandler handler) => _cachedHandlerTranslationOrNull = handler;
        
        private LastTranslationHandler _cachedHandlerTranslationOrNull = null;
        
        public bool CanBeHandled( CallbackQuery query) => query.Data.StartsWith(AddWordHelper.TranslationDataPrefix);

        public async Task Handle(Update update)
        {
            var buttonData = AddWordHelper.ParseQueryDataOrNull(update.CallbackQuery.Data);
            if (buttonData == null)
            {
                await _chat.ConfirmCallback(update.CallbackQuery.Id);
                return;
            }
            // last translation is cached. 
            var cached = _cachedHandlerTranslationOrNull;
            if (cached != null && cached.OriginWordText.Equals(buttonData.Origin))
            {
                // if translation is cached - fall into handler for fast handling
                await cached.Handle(buttonData.Translation, update);
                return;
            }
            
            // word is not cached
            // so we need to find already translated items
            var allTranslations = await _addWordService.FindInDictionaryWithExamples(buttonData.Origin);
            var originMessageButtons = update.CallbackQuery
                .Message
                ?.ReplyMarkup
                ?.InlineKeyboard
                ?.SelectMany(i => i)
                .ToArray();

            if (originMessageButtons == null)
            {
                await _chat.ConfirmCallback(update.CallbackQuery.Id);
                return;
            }

            if (originMessageButtons.Length < allTranslations.Count)
            {
                await _chat.ConfirmCallback(update.CallbackQuery.Id);
                return;
            }

            var selectionMarks = GetSelectionMarks(allTranslations, originMessageButtons);

            var index = AddWordHelper.FindIndexOf(allTranslations, buttonData.Translation);
            if(index==-1)
            {
                await _chat.ConfirmCallback(update.CallbackQuery.Id);
                return;
            }
                
            var selectedBefore = selectionMarks[index];
            selectionMarks[index] = true;
            await _addWordService.AddTranslationToUser(_user, allTranslations[index].GetEnRu(), 0);
            if (!selectedBefore)
            {
                await _chat.EditMessageButtons(
                    update.CallbackQuery.Message.MessageId,
                    allTranslations
                        .Select((t, i) => AddWordHelper.CreateButtonFor(t, selectionMarks[i]))
                        .ToArray());
            }

            await _chat.AnswerCallbackQueryWithTooltip(update.CallbackQuery.Id,
                AddWordHelper.GetMessageAfterTranslationIsSelected(allTranslations[index]));
        }

        private static bool[] GetSelectionMarks(IReadOnlyList<DictionaryTranslation> allTranslations, InlineKeyboardButton[] originMessageButtons)
        {
            bool[] selectionMarks = new bool[allTranslations.Count];
            int i = 0;
            foreach (var originMessageButton in originMessageButtons)
            {
                var data = AddWordHelper.ParseQueryDataOrNull(originMessageButton.CallbackData);
                if (data != null)
                {
                    if (allTranslations[i].TranslatedText.Equals(data.Translation) && data.IsSelected)
                        selectionMarks[i] = true;
                }

                i++;
            }

            return selectionMarks;
        }
    }
}