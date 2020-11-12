using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chotiskazal.ConsoleTesting.Services;
using Chotiskazal.DAL;
using Chotiskazal.DAL.Services;

namespace Chotiskazal.Bot.Questions
{
    public class RuPhraseSubstitudeExam : IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Ru phrase substitude";
        public async Task<ExamResult> Pass(Chat chat, ExamService service, UserWordForLearning word, UserWordForLearning[] examList)
        {
            if (!word.Phrases.Any())
                return ExamResult.Impossible;

            var phrase = word.Phrases.GetRandomItem();
            
            var replaced = phrase.PhraseRuTranslate.Replace(phrase.EnWord, "...");
            if (replaced == phrase.PhraseRuTranslate)
                return ExamResult.Impossible;

            var sb = new StringBuilder();
            
            sb.AppendLine($"\"{phrase.EnPhrase}\"");
            sb.AppendLine($" translated as ");
            sb.AppendLine($"\"{replaced}\"");
            sb.AppendLine();
            sb.AppendLine($"Enter missing word: ");
            
            while (true)
            {
                var enter = await chat.WaitUserTextInput();
                if (string.IsNullOrWhiteSpace(enter))
                    continue;
                if (string.CompareOrdinal(phrase.EnWord.ToLower().Trim(), enter.ToLower().Trim()) == 0)
                {
                    await service.RegistrateSuccessAsync(word);
                    return ExamResult.Passed;
                }

                await chat.SendMessage($"Origin phrase was \"{phrase.PhraseRuTranslate}\"");
                await service.RegistrateFailureAsync(word);
                return ExamResult.Failed;
            }
        }
    }
}