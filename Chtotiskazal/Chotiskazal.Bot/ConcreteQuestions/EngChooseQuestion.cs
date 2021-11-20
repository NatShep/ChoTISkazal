using System.Linq;
using System.Threading.Tasks;
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

        public async Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word,
            UserWordModel[] examList)
        {
            var originTranslation = word.Translations.ToList().GetRandomItemOrNull();
           
            var variants = examList.SelectMany(e => e.TextTranslations)
                .Where(e => !word.TextTranslations.Contains(e))
                .Distinct()
                .Shuffle()
                .Take(5)
                .Append(originTranslation.Word)
                .Shuffle()
                .ToList();

            var msg = $"\\=\\=\\=\\=\\=\\>   *{word.Word}*    \\<\\=\\=\\=\\=\\=\r\n" +
                      ""+chat.Texts.ChooseTheTranslation+"";
            
            await chat.SendMarkdownMessageAsync(msg, InlineButtons.CreateVariants(variants));

            var choice = await chat.TryWaitInlineIntKeyboardInput();
            if (choice == null)
                return QuestionResult.RetryThisQuestion;

            var selected = variants[choice.Value];
            return word.TextTranslations.Any(t=>t.AreEqualIgnoreCase(selected)) 
                ? QuestionResult.Passed(chat.Texts) 
                : QuestionResult.Failed(chat.Texts);
        }
    }
}