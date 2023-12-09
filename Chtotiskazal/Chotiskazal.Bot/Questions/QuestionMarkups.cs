using SayWhat.Bll.Strings;

namespace Chotiskazal.Bot.Questions;

public static class QuestionMarkups {
    private static readonly Markdown QuestionHeader = Markdown.Escaped($"---{Emojis.Question}---").ToMonoWord();
    
    public static Markdown FreeTemplateMarkdown(Markdown s) => QuestionHeader + Markdown.Escaped("\r\n") + s;

    public static Markdown TranslateTemplate(string word, string instruction)
        => QuestionHeader
               .NewLine() +
           word.ToSemiBoldMarkdown()
               .NewLine()
               .NewLine()
               .NewLine()
               .AddEscaped(instruction);

    public static Markdown TranslateTemplate(string word, string instruction, string example)
        => QuestionHeader
            .NewLine()
            .AddMarkdown(word.ToSemiBoldMarkdown())
            .NewLine()
            .AddBypassed("    ")
            .AddMarkdown(example.ToItalicMarkdown())
            .NewLine()
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
            .AddMarkdown($"\"{a}\"".ToSemiBoldMarkdown()).NewLine()
            .AddBypassed("    ").AddMarkdown(translatesAs.ToItalicMarkdown()).ToSemiBold().NewLine()
            .AddMarkdown($"\"{b}\"".ToSemiBoldMarkdown()).NewLine()
            .NewLine()
            .AddEscaped(instruction);
}

//todo cr - here and there. Sometimes you use .AddNewLine() and sometimes you use '+' operator
// it looks confusing. You can either - Create NewLine static property in MarkdownObjec
// either add methods like 'AddEscaped(...)', 'AddEscapedItalic()', AddEscapedBold etc
// but combination of these looks very confusing
//TODO answer i need more time for this =) 