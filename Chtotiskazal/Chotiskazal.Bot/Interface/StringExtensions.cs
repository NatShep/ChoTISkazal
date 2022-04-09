namespace Chotiskazal.Bot.Interface {
public static class StringExtensions {
    public static Markdown ToSemiBoldMarkdown(this string s) {
        return  Markdown.Escaped(s).ToSemiBold();
    }
    
    public static Markdown ToItalicMarkdown(this string s) {
        return  Markdown.Escaped(s).ToItalic();
    }

    public static Markdown ToPreFormattedMonoMarkdown(this string s) {
        return Markdown.Escaped(s).ToPreFormattedMono();
    }
    
    public static Markdown ToMonoMarkdown(this string s) {
        return Markdown.Escaped(s).ToMono();
    }
}
}