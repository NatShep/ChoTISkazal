using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.ChatFlows;
using SayWhat.Bll.Dto;
using SayWhat.Bll.Services;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.Hooks
{
    public class TranslationSelectedUpdateHook : IChatUpdateHook
    {
        private ChatRoom Chat { get; }
        private readonly AddWordService _addWordService;
        
        public TranslationSelectedUpdateHook(
            AddWordService addWordService, 
            ChatRoom chat)
        {
            Chat = chat;
            _addWordService = addWordService;
        }

        public void SetLastTranslationHandler(LastTranslationHandler handler) => _cachedHandlerTranslationOrNull = handler;
        
        private LastTranslationHandler _cachedHandlerTranslationOrNull = null;
        
        public bool CanBeHandled(Update update) => 
            update.CallbackQuery?.Data!=null 
            && update.CallbackQuery.Data.StartsWith(AddWordHelper.TranslationDataPrefix);

        public async Task Handle(Update update)
        {
            var buttonData = AddWordHelper.ParseQueryDataOrNull(update.CallbackQuery.Data);
            if (buttonData == null)
            {
                await Chat.ConfirmCallback(update.CallbackQuery.Id);
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
            var allTranslations = await _addWordService.FindInLocalDictionaryWithExamples(buttonData.Origin);
            var originMessageButtons = update.CallbackQuery
                .Message
                ?.ReplyMarkup
                ?.InlineKeyboard
                ?.SelectMany(i => i)
                .ToArray();

            if (originMessageButtons == null)
            {
                await Chat.ConfirmCallback(update.CallbackQuery.Id);
                return;
            }

            if (originMessageButtons.Length < allTranslations.Count)
            {
                await Chat.ConfirmCallback(update.CallbackQuery.Id);
                return;
            }

            var selectionMarks = GetSelectionMarks(allTranslations, originMessageButtons);

            var index = AddWordHelper.FindIndexOf(allTranslations, buttonData.Translation);
            if(index==-1)
            {
                await Chat.ConfirmCallback(update.CallbackQuery.Id);
                return;
            }
                
            var selectedBefore = selectionMarks[index];
            if (!selectedBefore)
            {
                selectionMarks[index] = true;
                await _addWordService.AddTranslationToUser(Chat.User, allTranslations[index].GetEnRu());
                await Chat.AnswerCallbackQueryWithTooltip(update.CallbackQuery.Id,
                    Chat.Texts.MessageAfterTranslationIsSelected(allTranslations[index]));
            }

            else
            {
                selectionMarks[index] = false;
                await _addWordService.RemoveTranslationFromUser(Chat.User, allTranslations[index].GetEnRu());
                await Chat.AnswerCallbackQueryWithTooltip(update.CallbackQuery.Id,
                    Chat.Texts.MessageAfterTranslationIsDeselected(allTranslations[index]));
            }
            await Chat.EditMessageButtons(
                update.CallbackQuery.Message.MessageId,
                allTranslations
                    .Select((t, i) => AddWordHelper.CreateButtonFor(t, selectionMarks[i]))
                    .ToArray());
        }

        private static bool[] GetSelectionMarks(IReadOnlyList<Translation> allTranslations, InlineKeyboardButton[] originMessageButtons)
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