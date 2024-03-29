using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SayWhat.Bll.Strings;

public static class StringExtensions {
    public static Markdown ToSemiBoldMarkdown(this string s) => 
        Markdown.Escaped(s).ToSemiBold();

    public static Markdown ToItalicMarkdown(this string s) => 
        Markdown.Escaped(s).ToItalic();

    public static Markdown ToPreFormattedMonoMarkdown(this string s) => 
        Markdown.Escaped(s).ToQuotationMono();

    public static Markdown ToMonoMarkdown(this string s) => 
        Markdown.Escaped(s).ToMono();

    public static Markdown ToBypassedMarkdown(this string s) => 
        Markdown.Bypassed(s);

    public static Markdown ToEscapedMarkdown(this string s) => 
        Markdown.Escaped(s);

    private static string GetWithStarredBody(this string origin, int start, int finish, out string replacedBody) {
        var size = origin.Length - start - finish;
        replacedBody = origin.Substring(start, size);
        return origin.Substring(0, start) + '*'.Repeat(size) + origin.Tail(finish);
    }

    public static string GetWithStarredBody(this string origin, StarredHardness hardness, out string replacedBody) {
        return hardness == StarredHardness.Easy
            ? origin.Length switch
            {
                <= 3 => GetWithStarredBody(origin, 1, 0, out replacedBody),
                <= 5 => GetWithStarredBody(origin, 1, 1, out replacedBody),
                <= 9 => GetWithStarredBody(origin, 2, 2, out replacedBody),
                _ => GetWithStarredBody(origin, 2, 2, out replacedBody)
            }
            : origin.Length switch
            {
                <= 7 => GetWithStarredBody(origin, 1, 0, out replacedBody),
                <= 11 => GetWithStarredBody(origin, 2, 0, out replacedBody),
                _ => GetWithStarredBody(origin, 2, 1, out replacedBody)
            };
    }

    public static string Repeat(this string targetString, int count) {
        var sb = new StringBuilder();
        for (int i = 0; i < count; i++) {
            sb.Append(targetString);
        }

        return sb.ToString();
    }

    public static char FirstSymbol(this string targetString) => targetString[0];

    public static char LastSymbol(this string targetString) => targetString[^1];

    public static string Tail(this string targetString, int count) =>
        count == 0 ? string.Empty : targetString.Substring(targetString.Length - count);

    public static string Repeat(this char targetChar, int count) => new(targetChar, count);

    public static (string, StringsCompareResult) GetClosestTo(this IReadOnlyList<string> translations,
        string translation) {
        if (translations == null || translations.Count == 0)
            return (null, StringsCompareResult.NotEqual);

        var bestOne = translations[0];
        var bestOneResult = bestOne.CheckCloseness(translation);
        for (int i = 1; i < translations.Count; i++) {
            var res = translations[i].CheckCloseness(translation);
            if (res > bestOneResult) {
                bestOne = translations[i];
                bestOneResult = res;
            }
        }

        return (bestOne, bestOneResult);
    }

    public static StringsCompareResult CheckCloseness(this string wordA, string wordB) {
        if (wordA == null || wordB == null)
            return wordA == wordB ? StringsCompareResult.Equal : StringsCompareResult.NotEqual;

        if (wordA.Length <= 3 || wordB.Length <= 3)
            return AreEqualIgnoreCase(wordA, wordB) ? StringsCompareResult.Equal : StringsCompareResult.NotEqual;

        var distance = Fastenshtein.Levenshtein.Distance(wordA.ToLower(), wordB.ToLower());
        if (distance == 0)
            return StringsCompareResult.Equal;
        //small mistakes: one mistake for each 4 letters
        //big   mistakes: one mistake for each 3 letters
        int length = Math.Min(wordA.Length, wordB.Length);
        if (distance <= length / 4)
            return StringsCompareResult.SmallMistakes;
        if (distance <= length / 3)
            return StringsCompareResult.BigMistakes;
        return StringsCompareResult.NotEqual;
    }

    public static string Capitalize(this string word) {
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

    public static bool AreEqualIgnoreCase(this string wordA, string wordB) =>
        wordA.Equals(wordB, StringComparison.OrdinalIgnoreCase);

    public static bool AreEqualIgnoreSmallMistakes(this string wordA, string wordB) {
        var m = CheckCloseness(wordA, wordB);
        return m == StringsCompareResult.Equal || m == StringsCompareResult.SmallMistakes;
    }
}

public enum StringsCompareResult {
    Equal = 3,
    SmallMistakes = 2,
    BigMistakes = 1,
    NotEqual = 0,
}

public enum StarredHardness {
    Hard,
    Easy
}