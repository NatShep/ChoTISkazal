using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SayWhat.Bll.Dto;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Users;
using Telegram.Bot.Types;

namespace Chotiskazal.Bot.ChatFlows
{
    public class ConcreteTranslationFastHandler
    {
        private readonly AddWordService _addWordService;
        private readonly ChatIO _chat;
        private readonly UserModel _user;
        private int _selectedTranslationsCount = 0;
        private readonly IReadOnlyList<DictionaryTranslation> _translations;
        private bool _isLastMessageInTheChat =true ;
        public string OriginWordText { get;  }
        
        private readonly bool[] _areSelected;
        
        
        public ConcreteTranslationFastHandler(
            IReadOnlyList<DictionaryTranslation> translations, 
            UserModel user, 
            ChatIO chat, 
            AddWordService addWordService)
        {
            OriginWordText = translations[0].OriginText;
            _translations = translations;
            _areSelected = new bool[_translations.Count];
            _user = user;
            _chat = chat;
            _addWordService = addWordService;
        }

        public async Task Handle(string translation, Update update)
        {
            var index = AddWordHelper.FindIndexOf(_translations, translation);
            if(index==-1)
                return;
            if(_areSelected[index]) 
                return;
            _areSelected[index] = true;
            _selectedTranslationsCount++;
            await _addWordService.AddTranslationToUser(_user, _translations[index].GetEnRu(), 0);
            
            await _chat.EditMessageButtons(
                update.CallbackQuery.Message.MessageId,
                _translations.Select((t, i) => AddWordHelper.CreateButtonFor(t, _areSelected[i])).ToArray()
            );
            if (_isLastMessageInTheChat) 
                await _chat.SendMessageAsync($"Translation {translation} is saved");
        }

        public async Task OnNextUserMessage()
        {
            _isLastMessageInTheChat = false;
            if (_selectedTranslationsCount == 0)
            {
                // if user did not select the word before next choose
                // than we automaticly add first translation
                // but we can not be sure that it is new word for user
                // so we give score '3' to that pair, witch means 'familiar word'
                // and user needs at least one or two exams to pass the word
                var selected = _translations[0].GetEnRu();
                await _addWordService.AddTranslationToUser(_user, selected, 3);
            }
        }
        

    
    }
}