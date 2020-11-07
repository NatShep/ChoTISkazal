using Chotiskazal.ConsoleTesting.Services;
using Chotiskazal.DAL;

namespace Chotiskazal.ApI.Exams
{
    public interface IExam
    {
        bool NeedClearScreen { get; }
        string Name { get; }
        ExamResult Pass(ExamService service, UserWordForLearning word, UserWordForLearning[] examList);
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
