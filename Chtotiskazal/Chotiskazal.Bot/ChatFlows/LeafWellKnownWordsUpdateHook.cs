using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chotiskazal.Bot.InterfaceLang;
using SayWhat.Bll.Dto;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Users;
using SayWhat.MongoDAL.Words;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.ChatFlows
{
    public class LeafWellKnownWordsUpdateHook: IChatUpdateHook
    {
        private ChatRoom Chat { get; }
        private List<List<UserWordModel>> _wellKnownWords = new List<List<UserWordModel>>(); 
        private int _numberOfPaginate;

        private int NumberOfPaginate
        {
            get => _numberOfPaginate;
            set
            {
                if (value >= _wellKnownWords?.Count)
                    _numberOfPaginate = _wellKnownWords.Count-1;
                else if (value <0)
                    _numberOfPaginate = 0;
                else 
                    _numberOfPaginate = value;
            }
        }

        public LeafWellKnownWordsUpdateHook(ChatRoom chat)
        {
            
            Chat = chat;
        }

        public void SetWellKnownWords(List<List<UserWordModel>> wellKnownWords) => _wellKnownWords = wellKnownWords;
        public void SetNumberOfPaginate(int i) => NumberOfPaginate = i;

        public bool CanBeHandled(Update update)
        {
            var text = update.CallbackQuery?.Data;
            return text == "/>>" || text == "/<<";
        }

        public async Task Handle(Update update)
        {
            if (!_wellKnownWords.Any())
            {
                await Chat.ConfirmCallback(update.CallbackQuery.Id);
                return;
            }
                
            if (update.CallbackQuery.Data == "/<<")
                NumberOfPaginate--;
            else
                NumberOfPaginate++;

            var msg = new StringBuilder();
            foreach (var word in _wellKnownWords[NumberOfPaginate])
            {
                msg.Append(Emojis.ShowWellLearnedWords + " *" + word.Word + ":* " + word.AllTranslationsAsSingleString + "\r\n");
            }
            msg.Append(Chat.Texts.ShowNumberOfLists(_numberOfPaginate+1,_wellKnownWords.Count));
         
            await Chat.EditMessageTextMarkdown(
                update.CallbackQuery.Message.MessageId,
                msg.ToString(), 
                new[]
                {
                    new[]
                    {
                        new InlineKeyboardButton {CallbackData = "/<<", Text = "<<"},
                        new InlineKeyboardButton {CallbackData = "/>>", Text = ">>"},
                    },
                    new[]
                    {
                        InlineButtons.MainMenu($"{Emojis.MainMenu} {Chat.Texts.MainMenuButton}"),
                    }
                });
            
            await Chat.ConfirmCallback(update.CallbackQuery.Id);
        }
    }
}