using Chotiskazal.Logic.Services;
using Dic.Logic.DAL;

namespace Chotiskazal.ApI.Exams
{
    public interface IExam
    {
        bool NeedClearScreen { get; }
        string Name { get; }
        ExamResult Pass(NewWordsService service, PairModel word, PairModel[] examList);
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
