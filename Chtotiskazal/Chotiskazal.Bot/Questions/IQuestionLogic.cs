using System.Threading.Tasks;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.Questions;

public interface IQuestionLogic {
    Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList);
}

public enum QResult {
    Passed,
    Failed,
    Retry,
    Impossible,
}