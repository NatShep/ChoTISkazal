using System.Collections.Generic;
using SayWhat.Bll.Dto;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.ChatFlows
{
    public class TranslationButtonData
    {
        public TranslationButtonData(string origin, string translation, bool isSelected)
        {
            Origin = origin;
            Translation = translation;
            IsSelected = isSelected;
        }

        public string Origin { get; }
        public string Translation { get; }
        public bool IsSelected { get; }
    }
    public static class AddWordHelper
    {
        public const string Separator = "@";
        public const string TranslationDataPrefix = "/tr";
        public const string SelectedPrefix = "☑️ ";
        
        static string CreateButtonDataFor(DictionaryTranslation translation, bool isSelected)
            => TranslationDataPrefix 
               + translation.OriginText 
               + Separator 
               + translation.TranslatedText+Separator
               + (isSelected?"1":"0");

        public static TranslationButtonData ParseQueryDataOrNull(string buttonQueryData)
        {
            if (string.IsNullOrWhiteSpace(buttonQueryData))
                return null;
            if (!buttonQueryData.StartsWith(TranslationDataPrefix))
                return null;
            var splitted = buttonQueryData.Substring(3).Split(Separator);
            if (splitted.Length != 3)
                return null;
            return new TranslationButtonData(splitted[0],splitted[1], splitted[2]=="1");
        }
        public static InlineKeyboardButton CreateButtonFor(DictionaryTranslation translation, bool selected)
            => new InlineKeyboardButton {
                CallbackData = CreateButtonDataFor(translation,selected), 
                Text = selected
                    ? $"{SelectedPrefix}{translation.TranslatedText}"
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