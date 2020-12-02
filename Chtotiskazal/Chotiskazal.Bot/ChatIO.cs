using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
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

        private TaskCompletionSource<Update> _waitInputCompletionSource   = null;
        private TaskCompletionSource<string> _waitMessageCompletionSource = null;

        
        public ChatIO(TelegramBotClient client, Chat chat)
        {
            _client = client;
             ChatId = chat.Id;
        }

        private readonly string[] _menuItems = {"/help", "/stats", "/start", "/add", "/exam"};
       
        internal void HandleUpdate(Update args)
        {
            var msg = args.Message?.Text;
            if (!string.IsNullOrWhiteSpace(msg))
            {
                if (msg[0] == '/')
                {
                    if (!_menuItems.Contains(msg)) {
                        SendMessageAsync($"Invalid command '{msg}'").Wait();
                        return;
                    }

                    var textSrc = _waitMessageCompletionSource;
                    var objSrc = _waitInputCompletionSource;
                    _waitMessageCompletionSource = null;
                    _waitInputCompletionSource = null;
                    textSrc?.SetException(new ProcessInterruptedWithMenuCommand(msg));
                    objSrc?.SetException(new ProcessInterruptedWithMenuCommand(msg));
                    return;
                }
                if (_waitMessageCompletionSource != null)
                {
                    Botlog.WriteInfo($"Set text result", ChatId);
                    var src = _waitMessageCompletionSource;
                    _waitMessageCompletionSource = null;
                    src?.SetResult(args.Message.Text);
                    return;
                }
            }

            if (_waitInputCompletionSource != null)
            {
                Botlog.WriteInfo($"Set any result",ChatId);
                var src = _waitInputCompletionSource;
                _waitInputCompletionSource = null;
                src?.SetResult(args);
            }   
        }

        public Task SendTooltip(string tooltip) => _client.SendTextMessageAsync(ChatId, tooltip);
        public Task SendMessageAsync(string message)=> _client.SendTextMessageAsync(ChatId, message);
        public Task SendMessageAsync(string message, params InlineKeyboardButton[] buttons)
            => _client.SendTextMessageAsync(ChatId, message, replyMarkup:  new InlineKeyboardMarkup(buttons.Select(b=>new[]{b})));

        public async Task<Update> WaitUserInputAsync()
        {
            _waitInputCompletionSource = new TaskCompletionSource<Update>();
            Botlog.WriteInfo($"Wait for any",ChatId);
            var result = await _waitInputCompletionSource.Task;
            Botlog.WriteInfo($"Got any",ChatId);
            return result;
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
            Botlog.WriteInfo($"Wait for text",ChatId);
            _waitMessageCompletionSource = new TaskCompletionSource<string>();

            var result = await _waitMessageCompletionSource.Task;
            Botlog.WriteInfo($"Got text",ChatId);
            return result;
        }

        public Task SendTyping() => _client.SendChatActionAsync(ChatId, ChatAction.Typing, CancellationToken.None);
    }
}