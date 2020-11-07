using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.ConsoleTesting.Services;
using Chotiskazal.DAL;
using Chotiskazal.DAL.Services;

namespace Chotiskazal.Bot.Questions
{
    public class EngChoosePhraseExam : IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Eng Choose Phrase";

        public async Task<ExamResult> Pass(Chat chat, ExamService service, UserWordForLearning word, UserWordForLearning[] examList)
        {
            if (!word.Phrases.Any())
                return ExamResult.Impossible;
            
            var targetPhrase = word.Phrases.GetRandomItem();

            var other = examList.SelectMany(e => e.Phrases)
                .Where(p => !string.IsNullOrWhiteSpace(p?.EnPhrase) && p != targetPhrase)
                .Take(8);

            if(!other.Any())
                return ExamResult.Impossible;

            var variants = other
                .Append(targetPhrase)
                .Randomize()
                .Select(e => e.PhraseRuTranslate)
                .ToArray();
            
            var msg = $"=====>   {targetPhrase.EnPhrase}    <=====\r\nChoose the translation";
            await chat.SendMessage(msg, InlineButtons.CreateVariants(variants));
            
            var choice = await chat.TryWaitInlineIntKeyboardInput();
            if (choice == null)
                return ExamResult.Retry;
            
            if (variants[choice.Value] == targetPhrase.PhraseRuTranslate)
            {
                service.RegistrateSuccess(word);
                return ExamResult.Passed;
            }
            service.RegistrateFailure(word);
            return ExamResult.Failed;

            
            
            /*
            Console.WriteLine("=====>   " + targetPhrase.Origin + "    <=====");

            for (int i = 1; i <= variants.Length; i++)
            {
                Console.WriteLine($"{i}: " + variants[i - 1]);
            }

            Console.Write("Choose the translation: ");

            var selected = Console.ReadLine();
            if (selected.ToLower().StartsWith("e"))
                return ExamResult.Exit;

            if (!int.TryParse(selected, out var selectedIndex) || selectedIndex > variants.Length ||
                selectedIndex < 1)
                return ExamResult.Retry;

            if (variants[selectedIndex - 1] == targetPhrase.Translation)
            {
                service.RegistrateSuccess(word);
                return ExamResult.Passed;
            }
            service.RegistrateFailure(word);
            return ExamResult.Failed;*/

        }
    }
}