using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions;

public class PhraseAssembleWithClueScenario : IQuestionScenario {
    public QuestionInputType InputType => QuestionInputType.NeedsEnInput;
    public ScenarioWordTypeFit Fit => ScenarioWordTypeFit.OnlyPhrase;

    public async Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList) {
        var targetPhrase = word.Word;

        return await AssemblePhraseScenarioHelper.AssemblePhrase(chat, targetPhrase, word.RuTranslations.First().Word);
    }
}