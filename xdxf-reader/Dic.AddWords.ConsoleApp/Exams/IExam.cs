using System.Collections.Generic;
using System.Text;
using Dic.Logic.DAL;
using Dic.Logic.Services;

namespace Dic.AddWords.ConsoleApp
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
        Exit
    }
}
