using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions;

public class EngChooseQuestion:IQuestion
{
    public bool NeedClearScreen => false;
    public string Name => "Eng Choose";
    public double PassScore => 0.4;
    public double FailScore => 1;
    public async Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList)
    {
        var originTranslation = word.RuTranslations.ToList().GetRandomItemOrNull();
        var variants = examList.GetRuVariants(originTranslation, 5);

        var choice = await QuestionHelper.ChooseVariantsFlow(chat, word.Word, variants);
        if (choice==null)
            return QuestionResult.RetryThisQuestion;

        return word.TextTranslations.Any(t=>t.AreEqualIgnoreCase(choice))
            ? QuestionResult.Passed(chat.Texts)
            : QuestionResult.Failed(chat.Texts);
    }
}