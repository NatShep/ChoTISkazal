using System;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Chotiskazal.Bot.ChatFlows;
using SayWhat.Bll;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot
{
    public class ChatIO
    {
        private readonly TelegramBotClient _client;
        public readonly ChatId ChatId;
        private readonly Channel<Update> _senderChannel;

        public ChatIO(TelegramBotClient client, Chat chat)
        {
            _client = client;
            ChatId = chat.Id;
            _senderChannel = Channel.CreateBounded<Update>(
                new BoundedChannelOptions(3)
                    {SingleReader = true, SingleWriter = true});
        }

        private readonly string[] _menuItems = {"/help", "/stats", "/start", "/add", "/learn"};
        private IChatQueryHandler _queryHandler;

        public void RegistrateCallbackQueryHandler(IChatQueryHandler queryHandler)
        {
            _queryHandler = queryHandler;
        }

        internal void OnUpdate(Update args)
        {
            Botlog.WriteInfo($"Received msg from chat {this.ChatId.Identifier} {this.ChatId.Username}", false);
            if (args.CallbackQuery != null && _queryHandler?.CanBeHandled(args.CallbackQuery) == true)
            {
                var _ = _queryHandler.Handle(args);
            }
            else
                _senderChannel.Writer.TryWrite(args);

        }

        public Task SendTooltip(string tooltip) => _client.SendTextMessageAsync(ChatId, tooltip);
        public async Task<int> SendMessageAsync(string message)
        {
           var ans = await _client.SendTextMessageAsync(ChatId, message);
           return ans.MessageId;
        }

        public Task SendMessageAsync(string message, params InlineKeyboardButton[] buttons)
            => _client.SendTextMessageAsync(ChatId, message,
                replyMarkup: new InlineKeyboardMarkup(buttons.Select(b => new[] {b})));

        public Task SendMessageAsync(string message, InlineKeyboardButton[][] buttons)
            => _client.SendTextMessageAsync(ChatId, message, replyMarkup: new InlineKeyboardMarkup(buttons));

        public async Task<int> SendMarkdownMessageAsync(string message, params InlineKeyboardButton[] buttons)
        {
            var answer = await _client.SendTextMessageAsync(ChatId, message,
                replyMarkup: new InlineKeyboardMarkup(buttons.Select(b => new[] {b})),
                parseMode: ParseMode.MarkdownV2);
            return answer.MessageId;
        }

        public Task SendMarkdownMessageAsync(string message, InlineKeyboardButton[][] buttons)
            => _client.SendTextMessageAsync(ChatId, message,
                replyMarkup: new InlineKeyboardMarkup(buttons),
                parseMode: ParseMode.MarkdownV2);

        public async Task<Update> WaitUserInputAsync()
        {
            Botlog.WriteInfo($"Wait for update", ChatId, false);
            var upd = await _senderChannel.Reader.ReadAsync();
            string text = null;
            if (upd.CallbackQuery != null)
            {
                await _client.AnswerCallbackQueryAsync(upd.CallbackQuery.Id);
                text = upd.CallbackQuery.Data;
            }
            else
            {
                text = upd.Message?.Text;
            }
            
            if (_menuItems.Contains(text))
                throw new ProcessInterruptedWithMenuCommand(text);
            
            Botlog.WriteInfo($"Got update", ChatId, false);
            return upd;
        }

        public async Task<string> WaitInlineKeyboardInput()
        {
            while (true)
            {
                var res = await WaitUserInputAsync();
                if (res.CallbackQuery != null)
                    return res.CallbackQuery.Data;
            }
        }

        public async Task<int?> TryWaitInlineIntKeyboardInput()
        {
            var res = await WaitUserInputAsync();
            if (res.CallbackQuery != null && int.TryParse(res.CallbackQuery.Data, out var i))
                return i;

            return null;
        }

        public async Task<int> WaitInlineIntKeyboardInput()
        {
            while (true)
            {
                var res = await WaitUserInputAsync();
                if (res.CallbackQuery != null && int.TryParse(res.CallbackQuery.Data, out var i))
                    return i;
            }

        }

        public async Task<string> WaitUserTextInputAsync()
        {
            while (true)
            {
                var res = await WaitUserInputAsync();
                var txt = res.Message?.Text;
                if (!string.IsNullOrWhiteSpace(txt))
                {
                    return txt;
                }
            }
        }

        public Task SendTyping()
            => _client.SendChatActionAsync(ChatId, ChatAction.Typing, CancellationToken.None);

        public async Task<bool> EditMessageButtons(int messageId, InlineKeyboardButton[] buttons)
        {
            try
            {
                await _client.EditMessageReplyMarkupAsync(ChatId, messageId,
                    new InlineKeyboardMarkup(buttons.Select(b => new[] {b})));
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public async Task<bool> EditMessageText(int messageId, string newText)
        {
            try
            {
                await _client.EditMessageTextAsync(ChatId, messageId, newText );
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> AnswerCallbackQueryWithTooltip(string callbackQueryId, string s)
        {
            try
            {
                await _client.AnswerCallbackQueryAsync(callbackQueryId, s, false);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task ConfirmCallback(string callbackQueryId)
        {
            try
            {
                await _client.AnswerCallbackQueryAsync(callbackQueryId);
            }
            catch (Exception)
            {
            }
        }

       
    }
}