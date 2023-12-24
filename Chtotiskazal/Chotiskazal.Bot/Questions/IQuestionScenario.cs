using System.Threading.Tasks;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.Questions;

public interface IQuestionScenario {
    QuestionInputType InputType { get; }
    ScenarioWordTypeFit Fit { get; }
    Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList);
}

public enum ScenarioWordTypeFit {
    OnlyWord =1,
    OnlyPhrase = 2,
    WordAndPhrase = OnlyWord | OnlyPhrase
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