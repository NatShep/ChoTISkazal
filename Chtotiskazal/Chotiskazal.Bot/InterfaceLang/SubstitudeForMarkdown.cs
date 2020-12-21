namespace Chotiskazal.Bot.InterfaceLang
{
    public static class StringExtentionForMarkdown
    {
        public static string SubstituteForMarkdown(this string str)
        {
            str= str.Replace(".", "\\.");
            return str;
        }
        
    }
}