using System.Collections.Generic;
using System.IO;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot
{
    public static class InlineButtons
    {
        public static readonly InlineKeyboardButton Translation = new InlineKeyboardButton
            {CallbackData = "/add", Text = "Translate"};

        public static readonly InlineKeyboardButton Exam = new InlineKeyboardButton
            {CallbackData = "/learn", Text = "Learn"};
        
        public static InlineKeyboardButton ExamText(string text) => new InlineKeyboardButton
            {CallbackData = "/learn", Text = text};

        public static readonly InlineKeyboardButton Stats = new InlineKeyboardButton
            {CallbackData = "/stats", Text = "Stats"};

        public static InlineKeyboardButton HowToUse = new InlineKeyboardButton
            {CallbackData = "/help", Text = "Help"};

        public static InlineKeyboardButton MainMenu = new InlineKeyboardButton
            {CallbackData = "/start", Text = "Main menu"};

        public static InlineKeyboardButton[] CreateVariants(IEnumerable<string> variants)
            => variants.Select((v, i) => new InlineKeyboardButton
            {
                CallbackData = i.ToString(),
                Text = v?? throw new InvalidDataException("Keyboard text cannot be null")
            }).ToArray();
        
        public static InlineKeyboardButton[] CreateGhostVariants(IEnumerable<string> variants)
            => variants.Select((v) => new InlineKeyboardButton
            {
                CallbackData = "-",
                Text = v?? throw new InvalidDataException("Keyboard text cannot be null")
            }).ToArray();

        public static InlineKeyboardButton[] CreateVariantsWithCancel(IEnumerable<string> variants)
        {
            return variants.Select((v, i) => new InlineKeyboardButton
            {
                CallbackData = i.ToString(),
                Text = v
            }).Append(new InlineKeyboardButton
            {
                CallbackData = "/start",
                Text= "Cancel",
            }).ToArray();
        }
    }
}