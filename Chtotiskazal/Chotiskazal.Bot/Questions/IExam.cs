using System.Threading.Tasks;
using Chotiskazal.Bot.Services;
using Chotiskazal.Dal.DAL;

namespace Chotiskazal.Bot.Questions
{
    public interface IExam
    {
        bool NeedClearScreen { get; }
        string Name { get; }
        Task<ExamResult> Pass(ChatIO chatIo, ExamService service, UserWordForLearning word, UserWordForLearning[] examList);
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