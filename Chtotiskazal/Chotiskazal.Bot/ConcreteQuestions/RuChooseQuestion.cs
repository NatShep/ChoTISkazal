using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions;

public class RuChooseQuestion: IQuestion
{
    public bool NeedClearScreen => false;
    public string Name => "RuChoose";
    public double PassScore => 0.6;
    public double FailScore => 1;

    public async Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList)
    {
        string[] variants = QuestionHelper.GetEngVariants(examList, word.Word, 5);

        var ruWord = word.RuTranslations.First().Word;
        var choice = await QuestionHelper.ChooseVariantsFlow(chat, ruWord , variants);
        if (choice == null)
            return QuestionResult.RetryThisQuestion;

        return choice.AreEqualIgnoreCase(word.Word)
            ? QuestionResult.Passed(chat.Texts)
            : QuestionResult.Failed(chat.Texts);
    }
}