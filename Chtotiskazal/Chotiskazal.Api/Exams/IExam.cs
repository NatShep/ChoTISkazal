using Chotiskazal.Api.Models;
using Chotiskazal.ConsoleTesting.Services;

namespace Chotiskazal.ApI.Exams
{
    public interface IExam
    {
        bool NeedClearScreen { get; }
        string Name { get; }
        ExamResult Pass(ExamService service, WordForLearning word, WordForLearning[] examList);
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
