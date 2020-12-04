using System.Linq;

namespace SayWhat.Bll
{
    public static class SayWhatHelper
    {
        public static string Capitalize(this string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return word;
            var startletter = word[0];
            var capitalLetter = char.ToUpper(startletter);
            if (startletter == capitalLetter)
                return word;
            return capitalLetter + word.Substring(1);
        }
        public static bool IsRussian(this string englishWord) => englishWord.Count(e => e >= 'А' && e <= 'я') > 1;

    }
}