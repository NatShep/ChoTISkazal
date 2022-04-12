using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL;
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

            var msg = QuestionMarkups.TranslateTemplate(word.AllTranslationsAsSingleString, chat.Texts.ChooseTheTranslation);
            await chat.SendMarkdownMessageAsync(msg, InlineButtons.CreateVariants(variants));
            
            var choice = await chat.TryWaitInlineIntKeyboardInput();
            if (choice == null)
                return QuestionResult.RetryThisQuestion;
            
            return variants[choice.Value].AreEqualIgnoreCase(word.Word) 
                ? QuestionResult.Passed(chat.Texts) 
                : QuestionResult.Failed(chat.Texts);
        }
    }
}