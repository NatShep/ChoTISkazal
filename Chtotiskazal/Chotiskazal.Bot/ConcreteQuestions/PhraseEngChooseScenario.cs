using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions;

public class PhraseEngChooseScenario : IQuestionScenario {
    public QuestionInputType InputType => QuestionInputType.NeedsNoInput;
    public ScenarioWordTypeFit Fit => ScenarioWordTypeFit.OnlyPhrase;

    public async Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList) {
        var originTranslation = word.RuTranslations.ToList().GetRandomItemOrNull();
        var variants = examList.GetRuPhraseVariants(5, originTranslation.Word, 30);
        if(variants.Length<2)
            return QuestionResult.Impossible;
        
        var choice = await QuestionScenarioHelper.ChooseVariantsFlow(chat, word.Word, variants);
        if (choice == null)
            return QuestionResult.RetryThisQuestion;

        return word.TextTranslations.Any(t => t.AreEqualIgnoreCase(choice))
            ? QuestionResult.Passed(chat.Texts)
            : QuestionResult.FailedResultPhraseWas(word.TextTranslations.First(), chat.Texts);
    }
}