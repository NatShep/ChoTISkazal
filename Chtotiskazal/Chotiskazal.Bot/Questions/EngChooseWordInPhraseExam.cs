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

        public async Task<ExamResult> Pass(ChatIO chatIo, ExamService service, UserWordForLearning word, UserWordForLearning[] examList)
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
            var _ =chatIo.SendMessageAsync(sb.ToString(), InlineButtons.CreateVariants(variants));

            var choice = await chatIo.TryWaitInlineIntKeyboardInputAsync();
            if (choice == null)
                return ExamResult.Retry;

            if (variants[choice.Value] == word.EnWord)
            {
                await service.RegistrateSuccessAsync(word);
                return ExamResult.Passed;
            }

            await chatIo.SendMessageAsync($"Origin was: \"{phrase.EnPhrase}\"");
            await service.RegistrateFailureAsync(word);
            return ExamResult.Failed;
        }
    }
}