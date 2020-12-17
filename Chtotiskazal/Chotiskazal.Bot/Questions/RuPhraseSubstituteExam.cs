using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.Questions
{
    public class RuPhraseSubstituteExam : IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Ru phrase substitute";
        public async Task<QuestionResult> Pass(ChatIO chatIo, UserWordModel word,
            UserWordModel[] examList)
        {
            if (!word.Examples.Any())
                return QuestionResult.Impossible;

            var phrase = word.GetRandomExample();

            var replaced = phrase.TranslatedPhrase.Replace(phrase.TranslatedWord, "...");
            if (replaced == phrase.TranslatedPhrase)
                return QuestionResult.Impossible;

            var sb = new StringBuilder();

            sb.AppendLine($"\"{phrase.OriginPhrase}\"");
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
                var comparation = phrase.TranslatedWord.CheckForMistakes(enter.Trim());

                if (comparation== StringsCompareResult.Equal)
                    return QuestionResult.Passed;

                if (comparation == StringsCompareResult.SmallMistakes) {
                    await chatIo.SendMessageAsync("Almost right. But you have a typo. Let's try again");
                    return QuestionResult.Retry;
                }
                return QuestionResult.FailedText($"Origin phrase was '{phrase.TranslatedPhrase}'");
            }
        }
    }
}