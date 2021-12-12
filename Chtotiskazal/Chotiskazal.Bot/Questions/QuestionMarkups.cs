using System.Text;
using Chotiskazal.Bot.Interface;

namespace Chotiskazal.Bot.Questions {
public static class QuestionMarkups {
    private static readonly MarkdownObject QuestionHeader = MarkdownObject.Escaped("---❔---").ToPreFormattedMono();
    public static MarkdownObject FreeTemplateMarkdown(MarkdownObject s) => QuestionHeader + MarkdownObject.ByPassed("\r\n") + s;

    public static MarkdownObject TranslateTemplate(string word, string instruction)
        => QuestionHeader
               .AddNewLine() +
           MarkdownObject.Escaped(word).ToSemiBold()
               .AddNewLine()
               .AddNewLine()
               .AddNewLine()
               .AddEscaped(instruction);

    public static MarkdownObject TranslateTemplate(string word, string instruction, string example)
        => QuestionHeader
               .AddNewLine() +
           MarkdownObject.Escaped(word).ToSemiBold()
               .AddNewLine() +
           MarkdownObject.ByPassed("    ") + MarkdownObject.Escaped(example).ToItalic()
               .AddNewLine()
               .AddNewLine()
               .AddNewLine()
               .AddEscaped(instruction);

    public static MarkdownObject TranscriptionTemplate(string word, string instruction)
        => QuestionHeader
            .AddNewLine()
            .AddEscaped($"'{word}'")
            .AddNewLine()
            .AddNewLine()
            .AddEscaped(instruction);

    public static MarkdownObject TranslatesAsTemplate(string a, string translatesAs, string b, string instruction)
        => QuestionHeader
               .AddNewLine() +
           MarkdownObject.Escaped($"\"{a}\"").ToSemiBold()
               .AddNewLine() +
           MarkdownObject.ByPassed("    ") + MarkdownObject.Escaped(translatesAs).ToItalic()
               .AddNewLine() +
           MarkdownObject.Escaped($"\"{b}\"").ToSemiBold()
               .AddNewLine()
               .AddNewLine()
               .AddEscaped(instruction);
}
}