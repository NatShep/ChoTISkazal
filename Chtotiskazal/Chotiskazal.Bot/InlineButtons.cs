using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot
{
    public static class InlineButtons
    {
        public static readonly InlineKeyboardButton EnterWords = new InlineKeyboardButton
            {CallbackData = "~EnterWords", Text = "Enter words"};

        public static readonly InlineKeyboardButton Exam = new InlineKeyboardButton
            {CallbackData = "~Exam", Text = "Examination"};

        public static readonly InlineKeyboardButton Stats = new InlineKeyboardButton
            {CallbackData = "~Stats", Text = "Stats"};

        public static InlineKeyboardButton[] CreateVariants(IEnumerable<string> variants)=> variants.Select((v, i) => new InlineKeyboardButton
            {
                CallbackData = i.ToString(),
                Text = v
            }).ToArray();
        
        public static InlineKeyboardButton[] CreateVariantsWithCancel(IEnumerable<string> variants)
        {
            var res= new List<InlineKeyboardButton>();
            
            //todo cr variants.Select(...).Append(..).ToArray();
            res.AddRange(variants.Select((v, i) => new InlineKeyboardButton
            {
                CallbackData = i.ToString(),
                Text = v
            }).ToList());
            res.Add( new InlineKeyboardButton
            {
                CallbackData = "/start",
                Text= "Cancel",
            });
            return res.ToArray();
        }
    }
}