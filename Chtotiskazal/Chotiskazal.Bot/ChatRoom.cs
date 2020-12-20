using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Chotiskazal.Bot.InterfaceLang;
using SayWhat.MongoDAL.Users;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot
{
    public class ChatRoom
    {

        public IInterfaceTexts Texts { get; }
        public UserModel User { get; }
        private ChatIO _origin;
        public ChatId ChatId => _origin.ChatId;
        public ChatIO ChatIo => _origin;


        public ChatRoom(ChatIO origin, UserModel user)
        {
            _origin = origin;
            User = user;
            Texts = new EnglishTexts();
        }

        #region wrap chat io

        public Task<int> SendMessageAsync(string message)
            => _origin.SendMessageAsync(message);
        public Task SendMessageAsync(string message, params InlineKeyboardButton[] buttons)
            => _origin.SendMessageAsync(message, buttons);
        public Task SendMessageAsync(string message, InlineKeyboardButton[][] buttons)
            => _origin.SendMessageAsync(message, buttons);
        public Task<int> SendMarkdownMessageAsync(string message, params InlineKeyboardButton[] buttons)
            => _origin.SendMarkdownMessageAsync(message, buttons);
        public Task SendMarkdownMessageAsync(string message, InlineKeyboardButton[][] buttons)
            => _origin.SendMarkdownMessageAsync(message, buttons);
        public  Task<Update> WaitUserInputAsync()
            => _origin.WaitUserInputAsync();
        public Task<string> WaitInlineKeyboardInput()
            => _origin.WaitInlineKeyboardInput();
        public Task<int?> TryWaitInlineIntKeyboardInput()
             => _origin.TryWaitInlineIntKeyboardInput();
        public Task<int> WaitInlineIntKeyboardInput()
            => _origin.WaitInlineIntKeyboardInput();
        public Task<string> WaitUserTextInputAsync()
            => _origin.WaitUserTextInputAsync();
        public Task SendTyping()
            => _origin.SendTyping();
        public Task<bool> EditMessageButtons(int messageId, InlineKeyboardButton[] buttons)
            => _origin.EditMessageButtons(messageId, buttons);
        public Task<bool> EditMessageText(int messageId, string newText)
            => _origin.EditMessageText(messageId, newText);
        public Task<bool> AnswerCallbackQueryWithTooltip(string callbackQueryId, string s)
            => _origin.AnswerCallbackQueryWithTooltip(callbackQueryId, s);
        public Task ConfirmCallback(string callbackQueryId)
            => _origin.ConfirmCallback(callbackQueryId);
        #endregion
    }
}