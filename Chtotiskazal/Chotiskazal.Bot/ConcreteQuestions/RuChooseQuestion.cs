using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions
{
    public class RuChooseQuestion: IQuestion
    {
        public bool NeedClearScreen => false;

        public string Name => "RuChoose";

        public async Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList)
        {
            string[] variants = QuestionHelper.GetEngVariants(examList, word.Word, 5);

            var choice = await QuestionHelper.ChooseVariantsFlow(chat, word.Word, variants);
            if (choice == null)
                return QuestionResult.RetryThisQuestion;

            return choice.AreEqualIgnoreCase(word.Word)
                ? QuestionResult.Passed(chat.Texts)
                : QuestionResult.Failed(chat.Texts);
        }
    }
}