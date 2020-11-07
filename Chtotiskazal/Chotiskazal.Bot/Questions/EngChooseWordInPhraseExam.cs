using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chotiskazal.ConsoleTesting.Services;
using Chotiskazal.DAL;
using Chotiskazal.DAL.Services;

namespace Chotiskazal.Bot.Questions
{
    public class EngChooseWordInPhraseExam : IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Eng Choose word in phrase";

        public async Task<ExamResult> Pass(Chat chat, ExamService service, UserWordForLearning word, UserWordForLearning[] examList)
        {
            if (!word.Phrases.Any())
                return ExamResult.Impossible;
            
            var phrase = word.Phrases.GetRandomItem();

            var replaced = phrase.EnPhrase.Replace(phrase.EnWord, "...");
            if (replaced == phrase.EnPhrase)
                return ExamResult.Impossible;

            var sb = new StringBuilder();
            sb.AppendLine($"\"{phrase.PhraseRuTranslate}\"");
            sb.AppendLine();
            sb.AppendLine($" translated as ");
            sb.AppendLine();
            sb.AppendLine($"\"{replaced}\"");
            sb.AppendLine($"Choose missing word...");

            var variants = examList.Randomize().Select(e => e.EnWord).ToArray();
            var _ =chat.SendMessage(sb.ToString(), InlineButtons.CreateVariants(variants));

            var choice = await chat.TryWaitInlineIntKeyboardInput();
            if (choice == null)
                return ExamResult.Retry;

            if (variants[choice.Value] == word.EnWord)
            {
                service.RegistrateSuccess(word);
                return ExamResult.Passed;
            }

            await chat.SendMessage($"Origin was: \"{phrase.EnPhrase}\"");
            service.RegistrateFailure(word);
            return ExamResult.Failed;
        }
    }
}