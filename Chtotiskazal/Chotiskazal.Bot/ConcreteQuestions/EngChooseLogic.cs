using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions;

public class EngChooseLogic : IQuestionLogic {
    public async Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList) {
        var originTranslation = word.RuTranslations.ToList().GetRandomItemOrNull();
        var variants = examList.GetRuVariants(originTranslation, 5);

        var choice = await QuestionLogicHelper.ChooseVariantsFlow(chat, word.Word, variants);
        if (choice == null)
            return QuestionResult.RetryThisQuestion;

        return word.TextTranslations.Any(t => t.AreEqualIgnoreCase(choice))
            ? QuestionResult.Passed(chat.Texts)
            : QuestionResult.Failed(chat.Texts);
    }
}