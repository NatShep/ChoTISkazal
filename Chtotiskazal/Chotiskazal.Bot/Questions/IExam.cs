using System.Threading.Tasks;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.Questions
{
    public interface IExam
    {
        bool NeedClearScreen { get; }
        string Name { get; }
        Task<ExamResult> Pass(ChatIO chatIo, UserWordModel word, UserWordModel[] examList);
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