using System.Collections.Generic;
using System.IO;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot
{
    public static class InlineButtons
    {
        public static readonly InlineKeyboardButton EnterWords = new InlineKeyboardButton
            {CallbackData = "~EnterWords", Text = "Translate"};

        public static readonly InlineKeyboardButton Exam = new InlineKeyboardButton
            {CallbackData = "~Exam", Text = "Learn"};

        public static readonly InlineKeyboardButton Stats = new InlineKeyboardButton
            {CallbackData = "~Stats", Text = "Stats"};

        public static InlineKeyboardButton[] CreateVariants(IEnumerable<string> variants)
            => variants.Select((v, i) => new InlineKeyboardButton
            {
                CallbackData = i.ToString(),
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