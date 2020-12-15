using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.Bll.Dto;
using SayWhat.Bll.Services;
using SayWhat.Bll.Yapi;
using SayWhat.MongoDAL.Users;
using Telegram.Bot.Types;

namespace Chotiskazal.Bot.ChatFlows
{
    public class SelectWordTranslationCallbackQueryHandler : IChatCallbackQuery
    {
        private readonly AddWordService _addWordService;
        private readonly ChatIO _chat;
        private readonly UserModel _user;
        private readonly UsersWordsService _usersWordsService;
        public SelectWordTranslationCallbackQueryHandler(
            AddWordService addWordService, 
            ChatIO chat, 
            UserModel user, 
            UsersWordsService usersWordsService)
        {
            _addWordService = addWordService;
            _chat = chat;
            _user = user;
            _usersWordsService = usersWordsService;
        }

        public void SetTranslationHandler(ConcreteTranslationFastHandler handler) => _cachedHandlerTranslationFastOrNull = handler;
        
        private ConcreteTranslationFastHandler _cachedHandlerTranslationFastOrNull = null;
        
        public bool CanBeHandled( CallbackQuery query) => query.Data.StartsWith(AddWordHelper.TranslationDataPrefix);

        public async Task Handle(Update update)
        {
            var wordAndTranslation = update.CallbackQuery.Data
                .Substring(AddWordHelper.TranslationDataPrefix.Length)
                .Split(AddWordHelper.Separator);
            
            if(wordAndTranslation.Length!=2)
                return;
            
            var cached = _cachedHandlerTranslationFastOrNull;
            if (cached != null && cached.OriginWordText.Equals(wordAndTranslation[0]))
            {
                // if word is cached - fall into handler for fast handling
                await cached.Handle(wordAndTranslation[1], update);
                return;
            }
            // word is not cached
            // so we need to find already translated items
            
            var allTranslations = await _addWordService.FindInDictionaryWithExamples(wordAndTranslation[0]);

            bool[] selectionMarks;
            if (wordAndTranslation[0].IsRussian())
            {
                var enWord = wordAndTranslation[1];
                selectionMarks = await MarkAlreadySelectedWords(enWord, allTranslations);
            }
            else
            {
                var enWord = wordAndTranslation[0];
                selectionMarks =await MarkAlreadySelectedWords(enWord, allTranslations);
            }
            var index = AddWordHelper.FindIndexOf(allTranslations, wordAndTranslation[1]);
            if(index==-1) return;
            //if(selectionMarks[index]) return;
            selectionMarks[index] = true;
            await _addWordService.AddTranslationToUser(_user, allTranslations[index].GetEnRu(), 0);
            await _chat.EditMessageButtons(
                update.CallbackQuery.Message.MessageId, 
                allTranslations
                    .Select((t,i)=> AddWordHelper.CreateButtonFor(t, selectionMarks[i]))
                    .ToArray());
            await _chat.AnswerCallbackQueryWithTooltip(update.CallbackQuery.Id,
                $"Translation {allTranslations[index].TranslatedText} is saved");
        }

        private async Task<bool[]> MarkAlreadySelectedWords(string enWord,
            IReadOnlyList<DictionaryTranslation> allTranslations)
        {
            bool[] selectedMarks = new bool[allTranslations.Count]; 

            var alreadyContainedWord = await _usersWordsService.GetWordNullByEngWord(_user, enWord);
            if (alreadyContainedWord != null)
            {
                if (selectedMarks.Length == 1) selectedMarks[0] = true;
                else
                {
                    for (int i = 0; i < allTranslations.Count; i++)
                    {
                        if (alreadyContainedWord.Translations.Any(t => t.Word.Equals(allTranslations[i].TranslatedText))
                            )
                            //markAllAreadySelectedWords
                            selectedMarks[i] = true;
                    }
                }
            }

            return selectedMarks;
        }
    }
}