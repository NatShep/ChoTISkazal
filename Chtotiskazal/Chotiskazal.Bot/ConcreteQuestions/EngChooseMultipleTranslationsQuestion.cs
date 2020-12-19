using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.InterfaceLang;
using Chotiskazal.Bot.Questions;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions
{
    public class EngChooseMultipleTranslationsQuestion:IQuestion
    {
        public bool NeedClearScreen => false;

        public string Name => "Eng Choose";

        public async Task<QuestionResult> Pass(ChatIO chatIo,  UserWordModel word,
            UserWordModel[] examList)
        {
            var translations = string.Join(", ",word.TextTranslations.Randomize().Take(3));
            
            var variants = examList
                .Where(e => e.AllTranslationsAsSingleString != word.AllTranslationsAsSingleString)
                .Select(e => string.Join(", ", e.TextTranslations.Randomize().Take(3)))
                .Randomize()
                .Take(5)
                .Append(translations)
                .ToList();

            var msg = $"=====>   {word.Word}    <=====\r\n" +
                      Texts.Current.ChooseTheTranslation;
            await chatIo.SendMessageAsync(msg, InlineButtons.CreateVariants(variants));

            var choice = await chatIo.TryWaitInlineIntKeyboardInput();
            if (choice == null)
                return QuestionResult.Retry;

            var answer = variants[choice.Value].Split(",")
                .Select(e => e.Trim()).ToList();
            
            return !answer.Except(word.TextTranslations).Any() 
                ? QuestionResult.Passed 
                : QuestionResult.Failed;
        }
    }
}