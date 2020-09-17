

namespace Chotiskazal.ConsoleTesting.Exams
{
    public interface IExam
    {
        bool NeedClearScreen { get; }
        string Name { get; }
        ExamResult Pass();
    }

    public enum ExamResult
    {
        Passed,
        Failed,
        Retry,
        Impossible,
        Exit
    }
}
