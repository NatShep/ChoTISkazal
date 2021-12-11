

using System.Linq;

namespace Chotiskazal.Bot.Interface 
{

    public class MarkdownObject {
        
        private readonly string _str;
        private readonly string _markdownString;
        
        private MarkdownObject(string str, string markdownString) {
            _str = str;
            _markdownString = markdownString;
        }
        
        public static MarkdownObject Escaped(string str) {
            var escapedStr = MarkdownObject.ConvertToMarkdownString(str);
            return new MarkdownObject(str, escapedStr);        
        }

        public static MarkdownObject Empty() {
            return new MarkdownObject("", "");        
        }
        
        public static MarkdownObject ByPassed(string str) {
            return new MarkdownObject(str, str);
        }
        
        public static MarkdownObject operator +(MarkdownObject s1, MarkdownObject s2) {
            return MarkdownObject.ByPassed(s1.GetMarkdownString() + s2.GetMarkdownString());
        }
        
        public static bool IsNullOrEmpty(MarkdownObject s) {
            return s == null || s._str == "";
        }
        
        public string GetOrdinalString() {
            return _str;
        }
        
        public string GetMarkdownString() {
            return _markdownString;
        }

        public static string ConvertToMarkdownString(string str) {
            var a = str.Replace(@"\", "\\\\")
                    .Replace("'", "\\'")
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

public static class MarkdownExtension 
{
    public static MarkdownObject AddNewLine(this MarkdownObject m) {
        return m + MarkdownObject.ByPassed("\r\n");
    }
    
    public static MarkdownObject ToSemiBold(this MarkdownObject m) {
        return MarkdownObject.ByPassed("*")+m+MarkdownObject.ByPassed("*") ;
    }

    public static MarkdownObject ToItalic(this MarkdownObject m) {
        return MarkdownObject.ByPassed("_")+m+MarkdownObject.ByPassed("_") ;
    }
    public static MarkdownObject AddEscaped(this MarkdownObject m , string s) {
        return m+ MarkdownObject.Escaped(s) ;
    }
    
    public static MarkdownObject AddByPassed(this MarkdownObject m , string s) {
        return m+ MarkdownObject.ByPassed(s);
    }
}

}
