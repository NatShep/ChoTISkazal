using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Interface;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions
{
    public class EngChooseQuestion:IQuestion
    {
        public bool NeedClearScreen => false;

        public string Name => "Eng Choose";

        public async Task<QuestionResultMarkdown> Pass(ChatRoom chat, UserWordModel word,
            UserWordModel[] examList)
        {
            var originTranslation = word.RuTranslations.ToList().GetRandomItemOrNull();
           
            var variants = examList.SelectMany(e => e.TextTranslations)
                .Where(e => !word.TextTranslations.Contains(e))
                .Distinct()
                .Shuffle()
                .Take(5)
                .Append(originTranslation.Word)
                .Shuffle()
                .ToList();

            var msg = QuestionMarkups.TranslateTemplate(word.Word, chat.Texts.ChooseTheTranslation);
            
            await chat.SendMarkdownMessageAsync(msg, InlineButtons.CreateVariants(variants));

            var choice = await chat.TryWaitInlineIntKeyboardInput();
            if (choice == null)
                return QuestionResultMarkdown.RetryThisQuestion;

            var selected = variants[choice.Value];
            return word.TextTranslations.Any(t=>t.AreEqualIgnoreCase(selected)) 
                ? QuestionResultMarkdown.Passed(chat.Texts) 
                : QuestionResultMarkdown.Failed(chat.Texts);
        }
    }
}