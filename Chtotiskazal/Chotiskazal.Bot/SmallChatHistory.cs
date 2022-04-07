using System;
using System.Collections.Concurrent;
using Chotiskazal.Bot.Interface;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot {
public class SmallChatHistory {
    private readonly int _size;
    private readonly ConcurrentQueue<string> _buffer;
    
    public SmallChatHistory(int size) {
        _size = size;
        _buffer = new ConcurrentQueue<string>();
    }
    public string[] GetHistory() => _buffer.ToArray();
    public void OnInput(Update update) => SaveInput(ToMessage(update));
    public void OnEditMessageButtons(InlineKeyboardButton[] buttons) => SaveOutput($"Edit buttons [{buttons.Length}]");
    public void OnEditMessageButtons(InlineKeyboardButton[][] buttons) => SaveOutput($"Edit buttons [{buttons.Length} lines]");
    public void OnEditMessageText(string newText) => SaveOutput($"Edit: {newText}");
    public void OnEditMarkdownMessageText(Markdown newText) => SaveOutput($"Edit mrkd: {newText.GetMarkdownString()}");
    public void OnSendTyping() => SaveOutput("Typing...");
    public void OnAnswerWithTooltip(string s) => SaveOutput($"tooltip: {s}");
    public void InputConfirmCallback() { }
    public void OnOutputTooltip(string tooltip)  => SaveOutput($"tooltip 2: {tooltip}");
    public void OnOutputMessage(string message) => SaveOutput($"msg: {message}");
    public void OnOutputMessage(string message, InlineKeyboardButton[] buttons) => SaveOutput($"msg: {message} [{buttons.Length}]");
    public void OnOutputMessage(string message, InlineKeyboardButton[][] buttons) => SaveOutput($"msg: {message} [{buttons.Length} lines]");
    public void OnOutputMarkdownMessage(Markdown message, InlineKeyboardButton[] buttons) => SaveOutput($"msg mrkd: {message.GetMarkdownString()} [{buttons.Length}]");
    public void OnOutputMarkdownMessage(Markdown message, InlineKeyboardButton[][] buttons) => SaveOutput($"msg mrkd: {message.GetMarkdownString()} [{buttons.Length} lines]");
    
    private void SaveInput(string message) => Save($"[User {DateTime.Now.ToShortTimeString()}] {message}");
    private void SaveOutput(string message) => Save($"[Bot {DateTime.Now.ToShortTimeString()}] {message}");
    private void Save(string message) {
        _buffer.Enqueue(message);
        while (_buffer.Count>_size) {
            _buffer.TryDequeue(out _);
        }
    }
    private string ToMessage(Update update) {
        if (update.Message != null) return "message: "+ update.Message?.Text;
        if (update.InlineQuery != null)        return "inlineQuery: "+ update.InlineQuery?.Query;
        if (update.ChosenInlineResult != null) return "InlineResult: "+ update.ChosenInlineResult?.Query;
        if (update.CallbackQuery != null)      return "CallbackQuery: "+ update.CallbackQuery?.Data;
        if (update.EditedMessage != null) return "EditedMessage: "+ update.EditedMessage?.Caption;
        return $"Unknown: {update.Type}";
    }

}
}