using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chotiskazal.Bot.InterfaceLang;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot
{
    public static class InlineButtons
    {
        public static readonly InlineKeyboardButton Translation = new InlineKeyboardButton
            {CallbackData = "/add", Text = Texts.Current.TranslateButton};

        public static readonly InlineKeyboardButton Exam = new InlineKeyboardButton
            {CallbackData = "/learn", Text = Texts.Current.LearnButton};
        
        public static InlineKeyboardButton ExamText(string text) => new InlineKeyboardButton
            {CallbackData = "/learn", Text = text};

        public static readonly InlineKeyboardButton Stats = new InlineKeyboardButton
            {CallbackData = "/stats", Text = Texts.Current.StatsButton};

        public static InlineKeyboardButton HowToUse = new InlineKeyboardButton
            {CallbackData = "/help", Text = Texts.Current.HelpButton};

        public static InlineKeyboardButton MainMenu = new InlineKeyboardButton
            {CallbackData = "/start", Text = Texts.Current.MainMenuButton};

        public static InlineKeyboardButton[] CreateVariants(IEnumerable<string> variants)
            => variants.Select((v, i) => new InlineKeyboardButton
            {
                CallbackData = i.ToString(),
                Text = v?? throw new InvalidDataException("Keyboard text cannot be null")
            }).ToArray();
    }
}