using System;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.ConsoleTesting.Services;
using Chotiskazal.DAL;
using Chotiskazal.DAL.Services;
namespace Chotiskazal.Bot.Questions
{
    public class AssemblePhraseExam :IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Assemble phrase";

        public async Task<ExamResult> Pass(ChatIO chatIo, ExamService service, UserWordForLearning word, UserWordForLearning[] examList) 
        {
            if (!word.Phrases.Any())
                return ExamResult.Impossible;

            var targetPhrase = word.Phrases.GetRandomItem();

            string shuffled;
            while (true)
            {
                var split = 
                    targetPhrase.EnPhrase.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                if (split.Length < 2)
                    return ExamResult.Impossible;

                shuffled = string.Join(" ", split.Randomize());
                if(shuffled!= targetPhrase.EnPhrase)
                    break;
            }

            await chatIo.SendMessage("Words in phrase are shuffled. Write them in correct order:\r\n'" +  shuffled+ "'");
            string entry= null;
            while (string.IsNullOrWhiteSpace(entry))
            {
                entry = await chatIo.WaitUserTextInput();
                entry = entry.Trim();
            }

            if (string.CompareOrdinal(targetPhrase.EnPhrase, entry) == 0)
            {
                await service.RegistrateSuccessAsync(word);
                return ExamResult.Passed;
            }

            await chatIo.SendMessage($"Original phrase was: '{targetPhrase.EnPhrase}'");
            await service.RegistrateFailureAsync(word);
            return ExamResult.Failed;
        }
    }
}