

using System.Linq;

namespace Chotiskazal.Bot.Interface 
{
    //Todo cr [optional] Maybe it is better to rename it to 'Markdown'? Obviously it is an object ;)
    public class MarkdownObject {
        //todo cr - rename to sumething like 'bypass string' or 'raw' string to identify it  
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
        //todo cr as the markdown is immutable - make Empty just a static readonly property
        //No need to create it every time
        public static MarkdownObject Empty() {
            return new MarkdownObject("", "");        
        }
        //todo cr [optional] - consider to move all following methods to expression body
        //todo cr naming - Did you mean Bypassed? 
        public static MarkdownObject ByPassed(string str) {
            return new MarkdownObject(str, str);
        }
        
        public static MarkdownObject operator +(MarkdownObject s1, MarkdownObject s2) {
            return MarkdownObject.ByPassed(s1.GetMarkdownString() + s2.GetMarkdownString());
        }
        //todo cr - move to extension method, or instance method
        public static bool IsNullOrEmpty(MarkdownObject s) {
            return s == null || s._str == "";
        }
        
        public string GetOrdinalString() {
            return _str;
        }
        
        public string GetMarkdownString() {
            return _markdownString;
        }
        //todo cr - make it private
        public static string ConvertToMarkdownString(string str) {
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
//todo cr - move to separate file
//todo cr - move all methods to "expression body"
public static class MarkdownExtension 
{
    public static MarkdownObject AddNewLine(this MarkdownObject m) {
        return m + MarkdownObject.ByPassed("\r\n");
    }
    
    public static MarkdownObject ToSemiBold(this MarkdownObject m) {
        return MarkdownObject.ByPassed("*")+m+MarkdownObject.ByPassed("*") ;
    }
    //todo cr - what is the difference between ToPreFormattedMono and ToMono methods?
    public static MarkdownObject ToPreFormattedMono(this MarkdownObject m) {
        return MarkdownObject.ByPassed("```\r\n")+m+MarkdownObject.ByPassed("\r\n```") ;
    }
    //
    public static MarkdownObject ToMono(this MarkdownObject m) {
        return MarkdownObject.ByPassed("```\r\n")+m+MarkdownObject.ByPassed("\r\n```") ;
    }
    
    public static MarkdownObject ToItalic(this MarkdownObject m) {
        return MarkdownObject.ByPassed("_")+m+MarkdownObject.ByPassed("_") ;
    }
    public static MarkdownObject AddEscaped(this MarkdownObject m , string s) {
        return m+ MarkdownObject.Escaped(s) ;
    }
    //todo cr - did you mean AddBypassed?
    public static MarkdownObject AddByPassed(this MarkdownObject m , string s) {
        return m+ MarkdownObject.ByPassed(s);
    }
}

}
