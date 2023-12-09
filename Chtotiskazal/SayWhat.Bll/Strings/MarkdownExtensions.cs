namespace SayWhat.Bll.Strings;

public static class MarkdownExtension {
    public static Markdown NewLine(this Markdown m) => m.AddBypassed("\r\n");

    public static Markdown ToStrike(this Markdown m) => m.Wrap("~~");

    public static Markdown ToUnderline(this Markdown m) => m.Wrap("<u>", "</u>");

    public static Markdown ToBold(this Markdown m) => m.Wrap("**");

    public static Markdown ToSemiBold(this Markdown m) => m.Wrap("*");

    public static Markdown ToQuotationMono(this Markdown m) => m.Wrap("```\r\n", "\r\n```");

    public static Markdown ToMono(this Markdown m) => m.Wrap("`\r\n", "\r\n`");

    public static Markdown ToMonoWord(this Markdown m) => m.Wrap("`");

    public static Markdown ToMonoWord(this string word) => word.ToBypassedMarkdown().Wrap("`");

    public static Markdown ToItalic(this Markdown m) => m.Wrap("_");

    public static Markdown AddEscaped(this Markdown m, string s) => m + Markdown.Escaped(s);

    public static Markdown AddMarkdown(this Markdown m, Markdown markdown) => m + markdown;

    public static Markdown AddBypassed(this Markdown m, string s) => m + Markdown.Bypassed(s);

    public static bool IsNullOrEmpty(this Markdown s) => s == null || s.IsEmpty();

    private static Markdown Wrap(this Markdown m, string bypassedSymbol) {
        if (m.IsEmpty())
            return m;
        return Markdown.Bypassed(bypassedSymbol) + m + Markdown.Bypassed(bypassedSymbol);
    }

    private static Markdown Wrap(this Markdown m, string bypassedPrefix, string bypassedSuffix) {
        if (m.IsEmpty())
            return m;
        return Markdown.Bypassed(bypassedPrefix) + m + Markdown.Bypassed(bypassedSuffix);
    }
}