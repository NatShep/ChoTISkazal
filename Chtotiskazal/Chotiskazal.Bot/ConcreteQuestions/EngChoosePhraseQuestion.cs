using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Interface;
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

        public async Task<QuestionResultMarkdown> Pass(ChatRoom chat,
            UserWordModel word,
            UserWordModel[] examList)
        {
            if (!word.HasAnyExamples)
                return QuestionResultMarkdown.Impossible;
            
            var targetPhrase = word.GetRandomExample();

            var otherExamples = examList
                .SelectMany(e => e.Examples)
                .Where(p => !p.TranslatedPhrase.AreEqualIgnoreCase(targetPhrase.TranslatedPhrase))
                .Shuffle()
                .Take(5)
                .ToArray();

            if(!otherExamples.Any())
                return QuestionResultMarkdown.Impossible;
            
            var variants = otherExamples
                .Append(targetPhrase)
                .Select(e => e.TranslatedPhrase)
                .Shuffle()
                .ToArray();
            var msg = QuestionMarkups.TranslateTemplate(targetPhrase.OriginPhrase, chat.Texts.ChooseTheTranslation);
            
            await chat.SendMarkdownMessageAsync(msg, InlineButtons.CreateVariants(variants));
            
            var choice = await chat.TryWaitInlineIntKeyboardInput();
            if (choice == null)
                return QuestionResultMarkdown.RetryThisQuestion;
            
            return variants[choice.Value].AreEqualIgnoreCase(targetPhrase.TranslatedPhrase) 
                ? QuestionResultMarkdown.Passed(chat.Texts) 
                : QuestionResultMarkdown.Failed(chat.Texts);
        }
    }
}