using Chotiskazal.Bot.Interface;
using Chotiskazal.Bot.Interface.InterfaceTexts;

namespace Chotiskazal.Bot.Questions
{
    public class QuestionResultMarkdown
    {
        private readonly MarkdownObject _openResultsText;
        private readonly MarkdownObject _resultsBeforeHideousText;
        public const string NoText = "";
        
        
        private QuestionResultMarkdown(MarkdownObject openResultsText, MarkdownObject resultsBeforeHideousText, ExamResult results)
        {
            _openResultsText = openResultsText;
            _resultsBeforeHideousText = resultsBeforeHideousText;
            Results = results;
        }

        public static QuestionResultMarkdown Passed(MarkdownObject markdownText, IInterfaceTexts texts) 
            => new QuestionResultMarkdown(markdownText, MarkdownObject.Escaped(texts.PassedHideousDefault), ExamResult.Passed);
        public static QuestionResultMarkdown Failed(MarkdownObject markdownText, IInterfaceTexts texts)
            => new QuestionResultMarkdown(markdownText,MarkdownObject.Escaped(texts.FailedHideousDefault), ExamResult.Failed);
        public static QuestionResultMarkdown Passed(MarkdownObject text, MarkdownObject hideousText) 
            => new QuestionResultMarkdown(text, hideousText, ExamResult.Passed);
        public static QuestionResultMarkdown Failed(MarkdownObject text, MarkdownObject hideousText)
            => new QuestionResultMarkdown(text, hideousText, ExamResult.Failed);
        public static QuestionResultMarkdown Passed(IInterfaceTexts texts)=> new QuestionResultMarkdown(
            texts.PassedDefaultMarkdown,
            MarkdownObject.Escaped(texts.PassedHideousDefault), ExamResult.Passed);
        public static QuestionResultMarkdown Failed(IInterfaceTexts texts)=> new QuestionResultMarkdown(
            texts.FailedDefaultMarkdown,
            MarkdownObject.Escaped(texts.FailedHideousDefault), ExamResult.Failed);
        public static QuestionResultMarkdown RetryThisQuestion=> new QuestionResultMarkdown(MarkdownObject.Empty(),MarkdownObject.Empty(), ExamResult.Retry);
        public static QuestionResultMarkdown Impossible => new QuestionResultMarkdown(MarkdownObject.Empty(),MarkdownObject.Empty(), ExamResult.Impossible);

        private string Emoji => Results switch
        {
            ExamResult.Failed => Emojis.Failed,
            ExamResult.Passed => Emojis.Passed,
            _ => ""
        };
        
        /// <summary>
        /// Text with results, showing after question before next question
        /// </summary>
        public MarkdownObject OpenResultsTextMarkdown => MarkdownObject.IsNullOrEmpty(_openResultsText)
            ?MarkdownObject.Empty()
            :MarkdownObject.Escaped(Emoji+" ") +_openResultsText;

        /// <summary>
        /// Text with results, showing after question before next hideous question
        /// </summary>
        public MarkdownObject ResultsBeforeHideousTextMarkdown => MarkdownObject.IsNullOrEmpty(_resultsBeforeHideousText)
            ?MarkdownObject.Empty()
            :MarkdownObject.Escaped(Emoji +" ") + _resultsBeforeHideousText;

        public  ExamResult Results { get; }

    }
}