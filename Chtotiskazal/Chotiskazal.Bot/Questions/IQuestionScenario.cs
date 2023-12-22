using System.Threading.Tasks;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.Questions;

public interface IQuestionScenario {
    QuestionInputType InputType { get; }
    Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList);
}

public enum QuestionInputType {
    NeedsEnInput,
    NeedsRuInput,
    NeedsNoInput
}

public enum QResult {
    Passed,
    Failed,
    Retry,
    Impossible,
}