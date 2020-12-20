using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.InterfaceLang;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll;
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
            var variants = examList.Where(e=>e.AllTranslationsAsSingleString!=word.AllTranslationsAsSingleString)
                .Select(e => e.Word)
                .Take(5)
                .Append(word.Word)
                .Randomize()
                .ToArray();

            var msg = $"=====>   {word.AllTranslationsAsSingleString}    <=====\r\n" +
                        chat.Texts.ChooseTheTranslation;
            await chat.SendMessageAsync(msg, InlineButtons.CreateVariants(variants));
            
            var choice = await chat.TryWaitInlineIntKeyboardInput();
            if (choice == null)
                return QuestionResult.RetryThisQuestion;
            
            return variants[choice.Value].AreEqualIgnoreCase(word.Word) 
                ? QuestionResult.Passed(chat.Texts) 
                : QuestionResult.Failed(chat.Texts);
        }
    }
}