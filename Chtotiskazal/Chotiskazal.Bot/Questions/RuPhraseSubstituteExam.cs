using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chotiskazal.Bot.Services;
using SayWhat.Bll;
using SayWhat.Bll.Dto;

namespace Chotiskazal.Bot.Questions
{
    public class RuPhraseSubstituteExam : IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Ru phrase substitute";
        public async Task<ExamResult> Pass(ChatIO chatIo, ExamService service, UserWordModel word, UserWordModel[] examList)
        {
            if (!word.Phrases.Any())
                return ExamResult.Impossible;

            var phrase = word.GetRandomExample();
            
            var replaced = phrase.PhraseTranslation.Replace(phrase.WordTranslation, "...");
            if (replaced == phrase.PhraseTranslation)
                return ExamResult.Impossible;

            var sb = new StringBuilder();
            
            sb.AppendLine($"\"{phrase.OriginPhrase}\"");
            sb.AppendLine($" translated as ");
            sb.AppendLine($"\"{replaced}\"");
            sb.AppendLine();
            sb.AppendLine($"Enter missing word: ");
            
            while (true)
            {
                var enter = await chatIo.WaitUserTextInputAsync();
                if (string.IsNullOrWhiteSpace(enter))
                    continue;
                if (string.CompareOrdinal(phrase.OriginWord.ToLower().Trim(), enter.ToLower().Trim()) == 0)
                {
                    await service.RegisterSuccessAsync(word);
                    return ExamResult.Passed;
                }

                await chatIo.SendMessageAsync($"Origin phrase was \"{phrase.PhraseTranslation}\"");
                await service.RegisterFailureAsync(word);
                return ExamResult.Failed;
            }
        }
    }
}