using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot
{
    public class ChatIO
    {
        private readonly TelegramBotClient _client;
        public readonly ChatId ChatId;
        //todo cr - это не ответственность chatIO - перенести в ChatRoomFlow
        public readonly string UserFirstName;
        //todo cr - это не ответственность chatIO - перенести в ChatRoomFlow
        public readonly string UserLastName;

        private TaskCompletionSource<Update> _waitInputCompletionSource   = null;
        private TaskCompletionSource<string> _waitMessageCompletionSource = null;

        
        public ChatIO(TelegramBotClient client, Telegram.Bot.Types.Chat chat)
        {
            _client = client;
            //TODO chatId - тип ChatId, a Chat.Id тип лонг
            ChatId = chat.Id;
            UserFirstName = chat.FirstName;
            UserLastName = chat.LastName;
        }

        public string[] MenuItems = {"/help", "/stats", "/start", "/add", "/train"};
       
        
        internal void HandleUpdate(Update args)
        {
            string msg = args.Message?.Text;
            if (!string.IsNullOrWhiteSpace(msg))
            {
                if (msg[0] == '/')
                {
                    if (MenuItems.Contains(msg))
                    {
                        var textSrc = _waitMessageCompletionSource;
                        var objSrc = _waitInputCompletionSource;
                        _waitMessageCompletionSource = null;
                        _waitInputCompletionSource = null;
                        textSrc?.SetException(new ProcessInterruptedWithMenuCommand(msg));
                        objSrc?.SetException(new ProcessInterruptedWithMenuCommand(msg));
                        return;
                    }
                }
                if (_waitMessageCompletionSource != null)
                {
                    Botlog.Write("Set text result");
                    var src = _waitMessageCompletionSource;
                    _waitMessageCompletionSource = null;
                    src?.SetResult(args.Message.Text);
                    return;
                }
            }

            if (_waitInputCompletionSource != null)
            {
                Botlog.Write("Set any result");
                var src = _waitInputCompletionSource;
                _waitInputCompletionSource = null;
                src?.SetResult(args);
            }   
        }

        public Task SendTooltip(string tooltip) => _client.SendTextMessageAsync(ChatId, tooltip);
        public Task SendMessageAsync(string message)=> _client.SendTextMessageAsync(ChatId, message);
        public Task SendMessageAsync(string message, params InlineKeyboardButton[] buttons)
            => _client.SendTextMessageAsync(ChatId, message, replyMarkup:  new InlineKeyboardMarkup(buttons.Select(b=>new[]{b})));

     
        public Task SendMessageAsync(string message, params KeyboardButton[] buttons)
            => _client.SendTextMessageAsync(ChatId, message, replyMarkup:  new ReplyKeyboardMarkup(buttons.Select(b=>new[]{b}), oneTimeKeyboard:true));

        public Task SendMessageAsync(string message, IEnumerable<string> keyboard)
            => _client.SendTextMessageAsync(ChatId, message, replyMarkup:  
                new ReplyKeyboardMarkup(keyboard.Select(b=> new KeyboardButton(b)), 
                    oneTimeKeyboard:true, resizeKeyboard:true));

        
        public async Task<Update> WaitUserInputAsync()
        {
            _waitInputCompletionSource = new TaskCompletionSource<Update>();
            Botlog.Write("Wait for any");
            var result = await _waitInputCompletionSource.Task;
            Botlog.Write("Got any");
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

        public async Task<int?> TryWaitInlineIntKeyboardInputAsync()
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
        public async Task WaitInlineKeyboardInput(string expected)
        {
            while (true)
            {
                var res = await WaitUserInputAsync();
                if (res.CallbackQuery?.Data == expected)
                    return;
            }
        }

        public async Task<string> WaitUserTextInputAsync()
        {
            Botlog.Write("Wait for text");
            _waitMessageCompletionSource = new TaskCompletionSource<string>();

            var result = await _waitMessageCompletionSource.Task;
            Botlog.Write("Got text");
            return result;
        }

        public Task SendTodo([CallerMemberName] string caller = null) =>
            SendMessageAsync($"{caller} is not implemented yet ");
    }
}