using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chotiskazal.ConsoleTesting.Services;
using Chotiskazal.DAL;
using Chotiskazal.DAL.Services;

namespace Chotiskazal.Bot.Questions
{
    public class EngPhraseSubstitudeExam: IExam
    {
        public bool NeedClearScreen => false;
        public string Name => "Eng phrase substitude";
        public async Task<ExamResult> Pass(ChatIO chatIo, ExamService service, UserWordForLearning word, UserWordForLearning[] examList)
        {
            if (!word.Phrases.Any())
                return ExamResult.Impossible;

            var phrase =  word.Phrases.GetRandomItem();
            var replaced =  phrase.EnPhrase.Replace(phrase.EnWord, "...");
            if (replaced == phrase.EnPhrase)
                return ExamResult.Impossible;
            var sb = new StringBuilder();
            
            sb.AppendLine($"\"{phrase.PhraseRuTranslate}\"");
            sb.AppendLine($" translated as ");
            sb.AppendLine($"\"{replaced}\"");
            sb.AppendLine();
            sb.AppendLine($"Enter missing word: ");
            while (true)
            {
                var enter = await chatIo.WaitUserTextInput();
                if (string.IsNullOrWhiteSpace(enter))
                    continue;
                if (string.CompareOrdinal(word.EnWord.ToLower().Trim(), enter.ToLower().Trim()) == 0)
                {
                    await service.RegistrateSuccessAsync(word);
                    return ExamResult.Passed;
                }

                Console.ForegroundColor = ConsoleColor.Red;
                await chatIo.SendMessage($"Origin phrase was \"{phrase.EnPhrase}\"");
                return ExamResult.Failed;
            }
        }
    }
}
