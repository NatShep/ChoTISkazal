using System.Threading.Tasks;
using Chotiskazal.Bot.Services;
using SayWhat.Bll;
using SayWhat.Bll.Dto;

namespace Chotiskazal.Bot.Questions
{
    public interface IExam
    {
        bool NeedClearScreen { get; }
        string Name { get; }
        Task<ExamResult> Pass(ChatIO chatIo, ExamService service, UserWordModel word, UserWordModel[] examList);
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