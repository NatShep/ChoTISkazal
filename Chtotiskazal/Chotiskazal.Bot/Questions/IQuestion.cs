using System.Threading.Tasks;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.Questions;

public interface IQuestion
{
    bool NeedClearScreen { get; }
    string Name { get; }
    double PassScore { get; }
    double FailScore { get; }
    Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList);
}

public enum ExamResult
{
    Passed,
    Failed,
    Retry,
    Impossible,
}