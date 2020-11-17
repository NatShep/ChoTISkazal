using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chotiskazal.Bot.Services;
using Chotiskazal.Dal.DAL;
using Chotiskazal.DAL.Services;

namespace Chotiskazal.Bot.Questions
{
    public class EngPhraseSubstituteExam: IExam
    {
        public bool NeedClearScreen => false;
        public string Name => "Eng phrase substitute";
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
                var enter = await chatIo.WaitUserTextInputAsync();
                if (string.IsNullOrWhiteSpace(enter))
                    continue;
                if (string.CompareOrdinal(word.EnWord.ToLower().Trim(), enter.ToLower().Trim()) == 0)
                {
                    await service.RegisterSuccessAsync(word);
                    return ExamResult.Passed;
                }

                Console.ForegroundColor = ConsoleColor.Red;
                await chatIo.SendMessageAsync($"Origin phrase was \"{phrase.EnPhrase}\"");
                return ExamResult.Failed;
            }
        }
    }
}
