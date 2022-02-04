

using System.Linq;

namespace Chotiskazal.Bot.Interface 
{
    public class Markdown {
        private readonly string _rawStr;
        private readonly string _markdownString;
        public static readonly Markdown Empty = new Markdown("", "");
        
        private Markdown(string rawStr, string markdownString) {
            _rawStr = rawStr;
            _markdownString = markdownString;
        }
        
        public static Markdown Escaped(string str) {
            var escapedStr = ConvertToMarkdownString(str);
            return new Markdown(str, escapedStr);        
        }
     
        //todo cr [optional] - consider to move all following methods to expression body
        //todo answer Escaped and Bypassed are method of creation Markdown. How To move them to expression?
        public static Markdown Bypassed(string str) {
            return new Markdown(str, str);
        }
        
        public static Markdown operator +(Markdown s1, Markdown s2) {
            return Bypassed(s1.GetMarkdownString() + s2.GetMarkdownString());
        }
        //todo cr - move to extension method, or instance method
        //todo answer - how to move to extension. It use private field
        public static bool IsNullOrEmpty(Markdown s) {
            return s == null || s._rawStr == "";
        }
        
        public string GetOrdinalString() {
            return _rawStr;
        }
        
        public string GetMarkdownString() {
            return _markdownString;
        }

        private static string ConvertToMarkdownString(string str) {
            var a = str.Replace(@"\", "\\\\")
                    .Replace("`","\\`")
                    .Replace("*", "\\*")
                    .Replace("_", "\\_")
                    .Replace("{", "\\{")
                    .Replace("}", "\\}")
                    .Replace("[", "\\[")
                    .Replace("]", "\\]")
                    .Replace("(", "\\(")
                    .Replace(")", "\\)")
                    .Replace("#", "\\#")
                    .Replace("+", "\\+")
                    .Replace("-", "\\-")
                    .Replace(".", "\\.")
                    .Replace("!", "\\!")
                    .Replace("\"", "\\\"")
                ;
            return a;
        }
    }
}
