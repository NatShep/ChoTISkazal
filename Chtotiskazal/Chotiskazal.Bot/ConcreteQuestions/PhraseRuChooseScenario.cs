using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions;

public class PhraseRuChooseScenario : IQuestionScenario {
    public QuestionInputType InputType => QuestionInputType.NeedsNoInput;
    public ScenarioWordTypeFit Fit => ScenarioWordTypeFit.OnlyPhrase;

    public async Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList) {
        var phrases = examList.GetEnPhraseVariants(
            count: 5,
            originEnPhrase: word.Word,
            maxLength: 30);

        var ruWord = word.RuTranslations.First().Word;
        var choice = await QuestionScenarioHelper.ChooseVariantsFlow(chat, ruWord, phrases);
        if (choice == null)
            return QuestionResult.RetryThisQuestion;

        return choice.AreEqualIgnoreCase(word.Word)
            ? QuestionResult.Passed(chat.Texts)
            : QuestionResult.FailedResultPhraseWas(word.Word, chat.Texts);
    }
}