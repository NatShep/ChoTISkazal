using Dic.Logic.DAL;
using Dic.Logic.Services;

namespace Chotiskazal.App.Exams
{
    public interface IExam
    {
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
