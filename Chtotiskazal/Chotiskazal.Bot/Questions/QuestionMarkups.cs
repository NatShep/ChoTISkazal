using System.Text;
using Chotiskazal.Bot.Interface;

namespace Chotiskazal.Bot.Questions {
public static class QuestionMarkups {
    private static readonly Markdown QuestionHeader = Markdown.Escaped("---❔---").ToPreFormattedMono();
    public static Markdown FreeTemplateMarkdown(Markdown s) => QuestionHeader + Markdown.Bypassed("\r\n") + s;

    public static Markdown TranslateTemplate(string word, string instruction)
        => QuestionHeader
               .NewLine() +
                word.ToSemiBold()
               .NewLine()
               .NewLine()
               .NewLine()
               .AddEscaped(instruction);

    public static Markdown TranslateTemplate(string word, string instruction, string example)
        => QuestionHeader
            .NewLine()
            .AddMarkdown(word.ToSemiBold()).NewLine()
            .AddBypassed("    ")
            .AddMarkdown(example.ToItalic()).NewLine()
            .NewLine()
            .NewLine()
            .AddEscaped(instruction);

    public static Markdown TranscriptionTemplate(string word, string instruction)
        => QuestionHeader
            .NewLine()
            .AddEscaped($"'{word}'")
            .NewLine()
            .NewLine()
            .AddEscaped(instruction);

    public static Markdown TranslatesAsTemplate(string a, string translatesAs, string b, string instruction)
        => QuestionHeader.NewLine()
            .AddMarkdown($"\"{a}\"".ToSemiBold()).NewLine()
            .AddBypassed("    ").AddMarkdown(translatesAs.ToItalic()).ToSemiBold().NewLine()
            .AddMarkdown($"\"{b}\"".ToSemiBold()).NewLine()
            .NewLine()
            .AddEscaped(instruction);
}

//todo cr - here and there. Sometimes you use .AddNewLine() and sometimes you use '+' operator
// it looks confusing. You can either - Create NewLine static property in MarkdownObjec
// either add methods like 'AddEscaped(...)', 'AddEscapedItalic()', AddEscapedBold etc
// but combination of these looks very confusing
//TODO answer i need more time for this =) 
}