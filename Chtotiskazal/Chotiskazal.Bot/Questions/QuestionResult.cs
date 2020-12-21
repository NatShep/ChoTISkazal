using Chotiskazal.Bot.InterfaceLang;

namespace Chotiskazal.Bot.Questions
{
    public class QuestionResult
    {
        private readonly string _openResultsText;
        private readonly string _resultsBeforeHideousText;
        public const string NoText = "";
        private const string PassedEmoji = "✅ ";
        private const string FailedEmoji = "❌ ";
        
        public QuestionResult(string openResultsText, string resultsBeforeHideousText, ExamResult results)
        {
            _openResultsText = openResultsText;
            _resultsBeforeHideousText = resultsBeforeHideousText;
            Results = results;
        }

        public static QuestionResult Passed(string text, IInterfaceTexts texts) 
            => new QuestionResult(text, texts.PassedHideousDefaultMarkdown, ExamResult.Passed);
        public static QuestionResult Failed(string text, IInterfaceTexts texts)
            => new QuestionResult(text,texts.FailedHideousDefaultMarkdown , ExamResult.Failed);
        public static QuestionResult Passed(string text, string hideousText) 
            => new QuestionResult(text, hideousText, ExamResult.Passed);
        public static QuestionResult Failed(string text, string hideousText)
            => new QuestionResult(text,hideousText , ExamResult.Failed);
        public static QuestionResult Passed(IInterfaceTexts texts)=> new QuestionResult(
            texts.PassedDefaultMarkdown,
            texts.PassedHideousDefaultMarkdown, ExamResult.Passed);
        public static QuestionResult Failed(IInterfaceTexts texts)=> new QuestionResult(
            texts.FailedDefaultMarkdown,
            texts.FailedHideousDefaultMarkdown, ExamResult.Failed);
        public static QuestionResult RetryThisQuestion=> new QuestionResult("","", ExamResult.Retry);
        public static QuestionResult Impossible => new QuestionResult("","", ExamResult.Impossible);

        private string Emoji => Results switch
        {
            ExamResult.Failed => FailedEmoji,
            ExamResult.Passed => PassedEmoji,
            _ => ""
        };
        
        /// <summary>
        /// Text with results, showing after question before next question
        /// </summary>
        public string OpenResultsText => string.IsNullOrWhiteSpace(_openResultsText)
            ?""
            :(Emoji+_openResultsText);

        /// <summary>
        /// Text with results, showing after question before next hideous question
        /// </summary>
        public string ResultsBeforeHideousText => string.IsNullOrWhiteSpace(_resultsBeforeHideousText)
            ?""
            :(Emoji+_resultsBeforeHideousText);

        public  ExamResult Results { get; }

    }
}