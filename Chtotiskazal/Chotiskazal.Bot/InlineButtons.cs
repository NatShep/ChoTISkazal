using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot
{
    public static class InlineButtons{
        public readonly static InlineKeyboardButton EnterWords = new InlineKeyboardButton{ CallbackData = "~EnterWords", Text = "Enter words"};
        public readonly static InlineKeyboardButton Exam = new InlineKeyboardButton{ CallbackData = "~Exam", Text = "Examination"};
        public readonly static InlineKeyboardButton Stats = new InlineKeyboardButton{ CallbackData = "~Stats", Text = "Stats"};
        public  static InlineKeyboardButton[] CreateVariants(IEnumerable<string> variants) =>
            variants.Select((v, i) => new InlineKeyboardButton
            {
                CallbackData = i.ToString(),
                Text = v
            }).ToArray();

    }
    
    public static class KeyboardButtons{
        public  static KeyboardButton[] CreateVariants(IEnumerable<string> variants) =>
            variants.Select((v, i) => new KeyboardButton
            {
                Text = v
            }).ToArray();

    }
}