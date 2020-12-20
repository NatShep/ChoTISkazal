using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chotiskazal.Bot.InterfaceLang;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot
{
    public static class InlineButtons
    {
        public static InlineKeyboardButton Translation(IInterfaceTexts texts) => new InlineKeyboardButton
            {CallbackData = "/add", Text = texts.TranslateButton};

        public static InlineKeyboardButton Exam(IInterfaceTexts texts) => new InlineKeyboardButton
            {CallbackData = "/learn", Text = texts.LearnButton};
        
        public static InlineKeyboardButton ExamText(string text) => new InlineKeyboardButton
            {CallbackData = "/learn", Text = text};

        public static InlineKeyboardButton Stats(IInterfaceTexts texts) => new InlineKeyboardButton
            {CallbackData = "/stats", Text = texts.StatsButton};

        public static InlineKeyboardButton HowToUse(IInterfaceTexts texts) => new InlineKeyboardButton
            {CallbackData = "/help", Text = texts.HelpButton};

        public static InlineKeyboardButton MainMenu(IInterfaceTexts texts) => new InlineKeyboardButton
            {CallbackData = "/start", Text = texts.MainMenuButton};

        public static InlineKeyboardButton[] CreateVariants(IEnumerable<string> variants)
            => variants.Select((v, i) => new InlineKeyboardButton
            {
                CallbackData = i.ToString(),
                Text = v?? throw new InvalidDataException("Keyboard text cannot be null")
            }).ToArray();
    }
}