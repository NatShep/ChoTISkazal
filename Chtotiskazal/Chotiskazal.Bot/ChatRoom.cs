using System;
using System.Threading.Tasks;
using Chotiskazal.Bot.Interface;
using Chotiskazal.Bot.Texts;
using SayWhat.MongoDAL.Users;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot
{
    public class ChatRoom
    {
        public  UserModel User { get; }
        private readonly ChatIO _origin;

        public IInterfaceTexts Texts => User.GetText();

        public  ChatId ChatId => _origin.ChatId;
        public  ChatIO ChatIo => _origin;

        public ChatRoom(ChatIO origin, UserModel user) {
            User = user ?? throw new ArgumentNullException(nameof(user));
            _origin = origin;
        }

        #region wrap chat io

        public Task<int> SendMessageAsync(string message)
            => _origin.SendMessageAsync(message);
        public Task SendMessageAsync(string message, params InlineKeyboardButton[] buttons)
            => _origin.SendMessageAsync(message, buttons);
        public Task SendMessageAsync(string message, InlineKeyboardButton[][] buttons)
            => _origin.SendMessageAsync(message, buttons);
        public Task<int> SendMarkdownMessageAsync(Markdown message, params InlineKeyboardButton[] buttons)
            => _origin.SendMessageAsync(message, buttons);
        public Task<int> SendMarkdownMessageAsync(Markdown message, InlineKeyboardButton[][] buttons)
            => _origin.SendMessageAsync(message, buttons);
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

        public async Task<string> WaitNonEmptyUserTextInputAsync() {
            while (true)
            {
                var input = await WaitUserTextInputAsync();
                if (!string.IsNullOrEmpty(input))
                    return input;
            }
        }
        public Task SendTyping()
            => _origin.SendTyping();
        public Task<bool> EditMessageButtons(int messageId, InlineKeyboardButton[] buttons)
            => _origin.EditMessageButtons(messageId, buttons);
        public Task<bool> EditMessageButtons(int messageId, InlineKeyboardButton[][] buttons)
            => _origin.EditMessageButtons(messageId, buttons);
        public Task<bool> EditMessageText(int messageId, string newText)
            => _origin.EditMessageText(messageId, newText);
        public Task<bool> AnswerCallbackQueryWithTooltip(string callbackQueryId, string s)
            => _origin.AnswerCallbackQueryWithTooltip(callbackQueryId, s);
        public Task ConfirmCallback(string callbackQueryId)
            => _origin.ConfirmCallback(callbackQueryId);
        
        public Task EditMessageTextMarkdown(int messageMessageId, Markdown s,
                                                  InlineKeyboardMarkup inlineKeyboardButtons = null)
            => _origin.EditMessageAsync(messageMessageId, s, inlineKeyboardButtons);
        #endregion


    }
}