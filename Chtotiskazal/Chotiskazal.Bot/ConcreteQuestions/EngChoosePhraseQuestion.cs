using System.Linq;
using System.Threading.Tasks;
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

        public async Task<QuestionResult> Pass(ChatRoom chat,
            UserWordModel word,
            UserWordModel[] examList)
        {
            if (!word.HasAnyExamples)
                return QuestionResult.Impossible;
            
            var targetPhrase = word.GetRandomExample();

            var otherExamples = examList
                .SelectMany(e => e.Examples)
                .Where(p => !p.TranslatedPhrase.AreEqualIgnoreCase(targetPhrase.TranslatedPhrase))
                .Shuffle()
                .Take(5)
                .ToArray();

            if(!otherExamples.Any())
                return QuestionResult.Impossible;
            
            var variants = otherExamples
                .Append(targetPhrase)
                .Select(e => e.TranslatedPhrase)
                .Shuffle()
                .ToArray();
            
            var msg = $"\\=\\=\\=\\=\\=\\>  *{targetPhrase.OriginPhrase}*  \\<\\=\\=\\=\\=\\=\\=\r\n" +
                      $"{chat.Texts.ChooseTheTranslation}";
            
            await chat.SendMarkdownMessageAsync(msg, InlineButtons.CreateVariants(variants));
            
            var choice = await chat.TryWaitInlineIntKeyboardInput();
            if (choice == null)
                return QuestionResult.RetryThisQuestion;
            
            return variants[choice.Value].AreEqualIgnoreCase(targetPhrase.TranslatedPhrase) 
                ? QuestionResult.Passed(chat.Texts) 
                : QuestionResult.Failed(chat.Texts);
        }
    }
}