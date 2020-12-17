using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.Questions
{
    public class EngPhraseSubstituteExam: IExam
    {
        public bool NeedClearScreen => false;
        public string Name => "Eng phrase substitute";
        public async Task<QuestionResult> Pass(ChatIO chatIo, UserWordModel word, UserWordModel[] examList)
        {
            if (!word.Examples.Any())
                return QuestionResult.Impossible;

            var phrase   =  word.GetRandomExample();
            
            var allWordsWithPhraseOfSimilarTranslate = examList
                .SelectMany(e => e.Examples)
                .Where(p => p.TranslatedPhrase.AreEqualIgnoreCase(phrase.TranslatedPhrase))
                .Select(e=>e.OriginWord)
                .ToList();
            
            var replaced =  phrase.OriginPhrase.Replace(phrase.OriginWord, "...");
            if (replaced == phrase.OriginPhrase)
                return QuestionResult.Impossible;
            
            var sb = new StringBuilder();
            
            sb.AppendLine($"\"{phrase.TranslatedPhrase}\"");
            sb.AppendLine($" translated as ");
            sb.AppendLine($"\"{replaced}\"");
            sb.AppendLine();
            sb.AppendLine($"Enter missing word: ");
            await chatIo.SendMessageAsync(sb.ToString());
            while (true)
            {
                var enter = await chatIo.WaitUserTextInputAsync();
                if (string.IsNullOrWhiteSpace(enter))
                    continue;
                var (_, comparation) = allWordsWithPhraseOfSimilarTranslate.GetClosestTo(enter.Trim());
                if (comparation == StringsCompareResult.Equal)
                    return QuestionResult.Passed;
                if (comparation == StringsCompareResult.SmallMistakes) {
                    await chatIo.SendMessageAsync("Almost right. But you have a typo. Let's try again");
                    return QuestionResult.Retry;
                }
                return QuestionResult.FailedText($"Wrong. Origin phrase was \"{phrase.OriginPhrase}\"");
            }
        }
    }
}
