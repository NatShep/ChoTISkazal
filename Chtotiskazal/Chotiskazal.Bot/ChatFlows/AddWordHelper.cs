using System.Collections.Generic;
using SayWhat.Bll.Dto;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.ChatFlows
{
    public static class AddWordHelper
    {
        public const string Separator = "$:$";
        public const string TranslationDataPrefix = "/tr";

        private static string CreateButtonDataFor(DictionaryTranslation translation)
            => TranslationDataPrefix + translation.OriginText + Separator + translation.TranslatedText;

        public static InlineKeyboardButton CreateButtonFor(DictionaryTranslation translation, bool selected)
            => new InlineKeyboardButton {
                CallbackData = CreateButtonDataFor(translation), 
                Text = selected
                    ? $"✅ {translation.TranslatedText}"
                    : translation.TranslatedText
            };
        public static  int  FindIndexOf(IReadOnlyList<DictionaryTranslation> translations, string translation)
        {
            for (int i = 0; i < translations.Count; i++)
            {
                if (translations[i].TranslatedText.Equals(translation))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}