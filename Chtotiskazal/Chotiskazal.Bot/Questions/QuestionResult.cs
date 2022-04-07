using Chotiskazal.Bot.Interface;
using Chotiskazal.Bot.Interface.InterfaceTexts;

namespace Chotiskazal.Bot.Questions
{
    public class QuestionResult
    {
        private readonly Markdown _openResultsText;
        private readonly Markdown _resultsBeforeHideousText;
        public const string NoText = "";
        
        private QuestionResult(Markdown openResultsText, Markdown resultsBeforeHideousText, ExamResult results)
        {
            _openResultsText = openResultsText;
            _resultsBeforeHideousText = resultsBeforeHideousText;
            Results = results;
        }

        public static QuestionResult Passed(Markdown markdownText, IInterfaceTexts texts) 
            => new QuestionResult(markdownText, Markdown.Escaped(texts.PassedHideousDefault), ExamResult.Passed);
        public static QuestionResult Failed(Markdown markdownText, IInterfaceTexts texts)
            => new QuestionResult(markdownText,Markdown.Escaped(texts.FailedHideousDefault), ExamResult.Failed);
        public static QuestionResult Passed(Markdown text, Markdown hideousText) 
            => new QuestionResult(text, hideousText, ExamResult.Passed);
        public static QuestionResult Failed(Markdown text, Markdown hideousText)
            => new QuestionResult(text, hideousText, ExamResult.Failed);
        public static QuestionResult Passed(IInterfaceTexts texts)=> new QuestionResult(
            texts.PassedDefault,
            Markdown.Escaped(texts.PassedHideousDefault), ExamResult.Passed);
        public static QuestionResult Failed(IInterfaceTexts texts)=> new QuestionResult(
            texts.FailedDefault,
            Markdown.Escaped(texts.FailedHideousDefault), ExamResult.Failed);
        public static QuestionResult RetryThisQuestion=> new QuestionResult(Markdown.Empty,Markdown.Empty, ExamResult.Retry);
        public static QuestionResult Impossible => new QuestionResult(Markdown.Empty,Markdown.Empty, ExamResult.Impossible);

        private string Emoji => Results switch
        {
            ExamResult.Failed => Emojis.Failed,
            ExamResult.Passed => Emojis.Passed,
            _ => ""
        };
        
        /// <summary>
        /// Text with results, showing after question before next question
        /// </summary>
        public Markdown OpenResultsTextMarkdown => _openResultsText.IsNullOrEmpty()
            ?Markdown.Empty
            :Markdown.Escaped(Emoji+" ") +_openResultsText;

        /// <summary>
        /// Text with results, showing after question before next hideous question
        /// </summary>
        public Markdown ResultsBeforeHideousTextMarkdown => _resultsBeforeHideousText.IsNullOrEmpty()
            ?Markdown.Empty
            :Markdown.Escaped(Emoji +" ") + _resultsBeforeHideousText;

        public  ExamResult Results { get; }

    }
}