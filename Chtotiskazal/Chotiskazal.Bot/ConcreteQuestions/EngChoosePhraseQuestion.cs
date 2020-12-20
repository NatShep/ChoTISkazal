using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.InterfaceLang;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions
{
    public class EngChoosePhraseQuestion : IQuestion
    {
        public bool NeedClearScreen => false;

        public string Name => "Eng Choose Phrase";

        public async Task<QuestionResult> Pass(
            ChatIO chatIo, 
            UserWordModel word, 
            UserWordModel[] examList)
        {
            if (!word.HasAnyExamples)
                return QuestionResult.Impossible;
            
            var targetPhrase = word.GetRandomExample();

            var otherExamples = examList
                .SelectMany(e => e.Examples)
                .Where(p => !p.TranslatedPhrase.AreEqualIgnoreCase(targetPhrase.TranslatedPhrase))
                .Randomize()
                .Take(5)
                .ToArray();

            if(!otherExamples.Any())
                return QuestionResult.Impossible;
            
            var variants = otherExamples
                .Append(targetPhrase)
                .Select(e => e.TranslatedPhrase)
                .Randomize()
                .ToArray();
            
            var msg = $"=====>   {targetPhrase.OriginPhrase}    <=====\r\n" +
                      Texts.Current.ChooseTheTranslation;
            
            await chatIo.SendMessageAsync(msg, InlineButtons.CreateVariants(variants));
            
            var choice = await chatIo.TryWaitInlineIntKeyboardInput();
            if (choice == null)
                return QuestionResult.RetryThisQuestion;
            
            return variants[choice.Value].AreEqualIgnoreCase(targetPhrase.TranslatedPhrase) 
                ? QuestionResult.Passed 
                : QuestionResult.Failed;
        }
    }
}