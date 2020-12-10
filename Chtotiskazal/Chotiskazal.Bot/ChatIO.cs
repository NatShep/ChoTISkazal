using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
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
       
        internal void OnUpdate(Update args)
        {
            Botlog.WriteInfo($"Received msg from chat {this.ChatId.Identifier} {this.ChatId.Username}");
            _senderChannel.Writer.TryWrite(args);
        }

        public Task SendTooltip(string tooltip) => _client.SendTextMessageAsync(ChatId, tooltip);
        public Task SendMessageAsync(string message)=> _client.SendTextMessageAsync(ChatId, message);
        public Task SendMessageAsync(string message, params InlineKeyboardButton[] buttons)
            => _client.SendTextMessageAsync(ChatId, message, replyMarkup:  new InlineKeyboardMarkup(buttons.Select(b=>new[]{b})));
        public Task SendMessageAsync(string message,  InlineKeyboardButton[][] buttons)
            => _client.SendTextMessageAsync(ChatId, message, replyMarkup:  new InlineKeyboardMarkup(buttons));

        public Task SendMarkdownMessageAsync(string message, params InlineKeyboardButton[] buttons)
            => _client.SendTextMessageAsync(ChatId, message, 
                replyMarkup:  new InlineKeyboardMarkup(buttons.Select(b=>new[]{b})),
                parseMode: ParseMode.MarkdownV2);
        public Task SendMarkdownMessageAsync(string message, InlineKeyboardButton[][] buttons)
            => _client.SendTextMessageAsync(ChatId, message, 
                replyMarkup:  new InlineKeyboardMarkup(buttons),
                parseMode: ParseMode.MarkdownV2);

        public async Task<Update> WaitUserInputAsync()
        {
            Botlog.WriteInfo($"Wait for update",ChatId);
            var upd = await _senderChannel.Reader.ReadAsync();
            var txt = upd.Message?.Text ?? upd.CallbackQuery?.Data;
            
            if(_menuItems.Contains(txt))
                throw new ProcessInterruptedWithMenuCommand(txt);

            Botlog.WriteInfo($"Got update",ChatId);
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

        public Task SendTyping() => _client.SendChatActionAsync(ChatId, ChatAction.Typing, CancellationToken.None);
    }
}