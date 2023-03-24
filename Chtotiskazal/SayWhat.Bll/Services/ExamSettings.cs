namespace SayWhat.Bll.Services
{
    public class ExamSettings
    {
        public int MinAdvancedExamMinQuestionAskedForOneWordCount { get; set; } = 2;
        public int MaxAdvancedExamMinQuestionAskedForOneWordCount { get; set; } = 4;
        public int MinAdvancedQuestionsCount { get; set; } = 8;
        public int MaxAdvancedQuestionsCount { get; set; } = 20;
        public int MinNewLearningWordsCountInOneExam { get; set; } = 3;
        public int MaxNewLearningWordsCountInOneExam { get; set; } = 5;
        public int MaxLearningWordsCountInOneExam { get; set; } = 13;
        public int MaxExamSize { get; set; } = 30;
        public int ExamsCountGoalForDay { get; set; } = 10;
        public int MinBestLearnedWords { get; set; } = 2;
        public int MinimumQuestionAsked { get; set; } = 6;
        public int MaxTranslationsInOneExam { get; } = 3;

    }
}