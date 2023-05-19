namespace SayWhat.Bll.Services
{
    public class ExamSettings
    {
        public int MinWordsQuestionsInOneExam { get; set; } = 1;
        public int MaxWordsQuestionsInOneExam { get; set; } = 3;
        public int MaxExamSize { get; set; } = 25;
        
        public int ExamsCountGoalForDay { get; set; } = 10;
        public int MinimumQuestionAsked { get; set; } = 6;
        public int MaxTranslationsInOneExam { get; set; } = 3;

        public int NewWordInOneExam { get; set; } = 3;
        public int LearningWordsInOneExam { get; set; } = 3;
        public int WellDoneWordsInOneExam { get; set; } = 3;
        public int LearnedWordsInOneExam { get; set; } = 1;
        public int LastAskedWordsInOneExam { get; set; } = 2;

        public int CountOfVariantsForChoose { get; set; } = 10;
    }
}