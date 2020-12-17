namespace Chotiskazal.Bot.Questions
{
    public class QuestionResult
    {
        private readonly string _openResultsText;
        private readonly string _resultsBeforeHideousText;
        public const string NoText = "";
        public const string PassedEmoji = "✅ ";
        public const string FailedEmoji = "❌ ";
        public const string SosoEmoji = "〰️";

        public QuestionResult(string openResultsText, string resultsBeforeHideousText, ExamResult results)
        {
            _openResultsText = openResultsText;
            _resultsBeforeHideousText = resultsBeforeHideousText;
            Results = results;
        }

        private const string defaultFailedString = "Noo...";
        private const string defaultPassedString = "It's right!";
        private const string defaultIgnoredString = "So so ...";

        private const string defaultHideousFailedString = "Last answer was wrong";
        private const string defaultHideousPassedString = "Last answer was right";
        private const string defaultHideousIgnoredString = "Not really";
        
        public static QuestionResult PassedText(string text, string hideousText = null) 
            => new QuestionResult(text, hideousText??defaultHideousPassedString, ExamResult.Passed);
        public static QuestionResult FailedText(string text, string hideousText = null)
            => new QuestionResult(text,hideousText??defaultHideousFailedString , ExamResult.Failed);
      
        public static QuestionResult Passed=> new QuestionResult(
            defaultPassedString,
            defaultHideousPassedString, ExamResult.Passed);
        public static QuestionResult Failed=> new QuestionResult(
            defaultFailedString,
            defaultHideousFailedString, ExamResult.Failed);
        public static QuestionResult Retry=> new QuestionResult("","", ExamResult.Retry);
        public static QuestionResult Impossible => new QuestionResult("","", ExamResult.Impossible);

        private string Emoji => Results switch
        {
            ExamResult.Failed => FailedEmoji,
            ExamResult.Ignored => SosoEmoji,
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