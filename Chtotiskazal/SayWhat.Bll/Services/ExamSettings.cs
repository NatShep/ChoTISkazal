namespace SayWhat.Bll.Services
{
    public class ExamSettings
    {
        public int MinAdvancedExamMinQuestionAskedCount { get; set; } = 5;
        public int MaxAdvancedExamMinQuestionAskedCount { get; set; } = 13;
        public int MaxAdvancedQuestionsCount     { get; set; } = 7;
        public int MinLearningWordsCountInOneExam { get; set; } = 9;
        public int MaxLearningWordsCountInOneExam { get; set; } = 9;
        public int MinTimesThatLearningWordAppearsInExam { get; set; } = 2;
        public int MaxTimesThatLearningWordAppearsInExam { get; set; } = 4;
        public int MaxExamSize { get; set; } = 32;
    }
}