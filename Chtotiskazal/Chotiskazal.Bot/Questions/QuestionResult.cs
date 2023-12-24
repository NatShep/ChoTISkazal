using Chotiskazal.Bot.Texts;
using SayWhat.Bll.Strings;

namespace Chotiskazal.Bot.Questions;

public class QuestionResult {
    private readonly Markdown _openResultsText;
    private readonly Markdown _resultsBeforeHideousText;

    private QuestionResult(Markdown openResultsText, Markdown resultsBeforeHideousText, QResult results) {
        _openResultsText = openResultsText;
        _resultsBeforeHideousText = resultsBeforeHideousText;
        Results = results;
    }

    public static QuestionResult Passed(Markdown markdownText, IInterfaceTexts texts) =>
        new(markdownText, Markdown.Escaped(texts.PassedHideousDefault), QResult.Passed);

    public static QuestionResult Passed(Markdown text, Markdown hideousText) =>
        new(text, hideousText, QResult.Passed);

    public static QuestionResult Passed(string text, Markdown hideousText) =>
        new(text.ToEscapedMarkdown(), hideousText, QResult.Passed);

    public static QuestionResult Passed(IInterfaceTexts texts) => new(
        texts.PassedDefault, Markdown.Escaped(texts.PassedHideousDefault), QResult.Passed);

    public static QuestionResult Failed(Markdown text, Markdown hideousText) =>
        new(text, hideousText, QResult.Failed);

    public static QuestionResult Failed(Markdown text, IInterfaceTexts texts) =>
        new(text, Markdown.Escaped(texts.FailedHideousDefault), QResult.Failed);

    public static QuestionResult FailedResultPhraseWas(string originPhrase, IInterfaceTexts texts) => Failed(
        texts.FailedOriginPhraseWas2 +
        Markdown.Escaped(":")
            .NewLine()
        + ($"\"{originPhrase}\"".ToSemiBoldMarkdown()),
        texts);


    public static QuestionResult Failed(string text, IInterfaceTexts texts) =>
        new(text.ToEscapedMarkdown(), Markdown.Escaped(texts.FailedHideousDefault), QResult.Failed);

    public static QuestionResult Failed(IInterfaceTexts texts) => new(
        texts.FailedDefault, Markdown.Escaped(texts.FailedHideousDefault), QResult.Failed);

    public static QuestionResult RetryThisQuestion =>
        new(Markdown.Empty, Markdown.Empty, QResult.Retry);

    public static QuestionResult Impossible =>
        new(Markdown.Empty, Markdown.Empty, QResult.Impossible);

    private string Emoji => Results switch
    {
        QResult.Failed => Emojis.Failed,
        QResult.Passed => Emojis.Passed,
        _ => ""
    };

    /// <summary>
    /// Text with results, showing after question before next question
    /// </summary>
    public Markdown OpenResultsTextMarkdown => _openResultsText.IsNullOrEmpty()
        ? Markdown.Empty
        : Markdown.Escaped(Emoji + " ") + _openResultsText;

    /// <summary>
    /// Text with results, showing after question before next hideous question
    /// </summary>
    public Markdown ResultsBeforeHideousTextMarkdown => _resultsBeforeHideousText.IsNullOrEmpty()
        ? Markdown.Empty
        : Markdown.Escaped(Emoji + " ") + _resultsBeforeHideousText;

    public QResult Results { get; }
}