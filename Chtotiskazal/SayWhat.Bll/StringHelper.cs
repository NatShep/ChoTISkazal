using System;
using System.Collections.Generic;
using System.Linq;
using SayWhat.Bll.Dto;

namespace SayWhat.Bll
{
    public enum StringsCompareResult
    {
        Equal = 3,
        SmallMistakes = 2,
        BigMistakes  = 1,
        NotEqual = 0,
    }
    public static class StringHelper
    {
        public static string EscapeForMarkdown(this string targetString) 
            => targetString.Replace(".", "\\.").Replace("-", "\\-").Replace("!","\\!");

        public static (string, StringsCompareResult) GetClosestTo(this IReadOnlyList<string> translations, string translation)
        {
            if(translations==null || translations.Count==0)
                return (null, StringsCompareResult.NotEqual);
            
            var bestOne  = translations[0];
            var bestOneResult =  bestOne.CheckCloseness(translation);
            for (int i = 1; i < translations.Count; i++)
            {
                var res = translations[i].CheckCloseness(translation);
                if (res > bestOneResult)
                {
                    bestOne = translations[i];
                    bestOneResult = res;
                }                
            }
            return (bestOne, bestOneResult);
        }
        public static (Translation, StringsCompareResult) GetClosestTranslation(Translation[] translations, string translation)
        {
            if(translations==null || translations.Length==0)
                return (null, StringsCompareResult.NotEqual);
            
            var bestOne  = translations[0];
            var bestOneResult =  bestOne.TranslatedText.CheckCloseness(translation);
            for (int i = 1; i < translations.Length; i++)
            {
                var res = translations[i].TranslatedText.CheckCloseness(translation);
                if (res > bestOneResult)
                {
                    bestOne = translations[i];
                    bestOneResult = res;
                }                
            }
            return (bestOne, bestOneResult);
        }
        public static bool AreEqualIgnoreCase(this string wordA, string wordB) => wordA.Equals(wordB, StringComparison.OrdinalIgnoreCase);
        public static bool AreEqualIgnoreSmallMistakes(this string wordA, string wordB)
        {
            var m =CheckCloseness(wordA, wordB);
            return m == StringsCompareResult.Equal || m == StringsCompareResult.SmallMistakes;
        }
        public static StringsCompareResult CheckCloseness(this string wordA, string wordB)
        {
            if(wordA==null || wordB==null)
                return wordA==wordB? StringsCompareResult.Equal: StringsCompareResult.NotEqual;
            
            if (wordA.Length <= 4 || wordB.Length <= 4)
                return AreEqualIgnoreCase(wordA, wordB) ? StringsCompareResult.Equal : StringsCompareResult.NotEqual;
            
            var distance = Fastenshtein.Levenshtein.Distance(wordA.ToLower(), wordB.ToLower());
            if (distance == 0)
                return StringsCompareResult.Equal;
            //small mistakes: one mistake for each 5 letters
            //big   mistakes: one mistake for each 3 letters
            int length = Math.Min(wordA.Length, wordB.Length);
            if (distance <= length / 5)
                return StringsCompareResult.SmallMistakes;
            if (distance <= length / 3)
                return StringsCompareResult.BigMistakes;
            return StringsCompareResult.NotEqual;
        }
        public static string Capitalize(this string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return word;
            word = word.ToLower();
            var startletter = word[0];
            var capitalLetter = char.ToUpper(startletter);
            if (startletter == capitalLetter)
                return word;
            return capitalLetter + word.Substring(1);
        }
        public static bool IsRussian(this string englishWord) => englishWord.Count(e => e >= 'А' && e <= 'я') > 1;

    }
}