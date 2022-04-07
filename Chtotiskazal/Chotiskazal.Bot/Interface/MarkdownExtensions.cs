namespace Chotiskazal.Bot.Interface {
public static class MarkdownExtension {
    public static Markdown NewLine(this Markdown m) {
        return m.AddBypassed("\r\n");
    }

    
    public static Markdown ToSemiBold(this Markdown m) {
        return Markdown.Bypassed("*") + m + Markdown.Bypassed("*");
    }

    public static Markdown ToPreFormattedMono(this Markdown m) {
        return Markdown.Bypassed("```\r\n") + m + Markdown.Bypassed("\r\n```");
    }

    public static Markdown ToMono(this Markdown m) {
        return Markdown.Bypassed("`\r\n") + m + Markdown.Bypassed("\r\n`");
    }

    public static Markdown ToItalic(this Markdown m) {
        return Markdown.Bypassed("_") + m + Markdown.Bypassed("_");
    }

    public static Markdown AddEscaped(this Markdown m, string s) {
        return m + Markdown.Escaped(s);
    }

    public static Markdown AddMarkdown(this Markdown m, Markdown markdown) {
        return m + markdown;
    }

    public static Markdown AddBypassed(this Markdown m, string s) {
        return m + Markdown.Bypassed(s);
    }

    public static bool IsNullOrEmpty(this Markdown s) {
        return s == null || s.IsEmpty();
    }
}
}