namespace Chotiskazal.Bot.Interface {
public static class StringExtensions {
    public static Markdown ToSemiBold(this string s) {
        return  Markdown.Escaped(s).ToSemiBold();
    }
    
    public static Markdown ToItalic(this string s) {
        return  Markdown.Escaped(s).ToItalic();
    }
}
}