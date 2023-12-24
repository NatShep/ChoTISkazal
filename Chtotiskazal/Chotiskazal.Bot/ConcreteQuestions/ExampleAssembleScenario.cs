using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions;

public class ExampleAssembleScenario : IQuestionScenario {
    public QuestionInputType InputType => QuestionInputType.NeedsEnInput;
    public ScenarioWordTypeFit Fit => ScenarioWordTypeFit.OnlyWord;

    public async Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList) {
        if (!word.HasAnyExamples)
            return QuestionResult.Impossible;
        var targetPhrase = word.GetRandomExample();
        return await AssemblePhraseScenarioHelper.AssemblePhrase(chat, targetPhrase.OriginPhrase);
    }
}