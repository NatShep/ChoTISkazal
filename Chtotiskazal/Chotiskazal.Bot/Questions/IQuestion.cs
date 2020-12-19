using System.Threading.Tasks;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.Questions
{
    public interface IQuestion
    {
        bool NeedClearScreen { get; }
        string Name { get; }
        Task<QuestionResult> Pass(ChatIO chatIo, UserWordModel word, UserWordModel[] examList);
    }

    public enum ExamResult
    {
        Passed,
        Failed,
        Retry,
        Impossible,
        Ignored,
    }
}