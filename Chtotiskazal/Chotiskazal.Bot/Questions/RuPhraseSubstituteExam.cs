using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.Bll.Dto;
using SayWhat.Bll.Services;

namespace Chotiskazal.Bot.Questions
{
    public class RuPhraseSubstituteExam : IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Ru phrase substitute";
        public async Task<ExamResult> Pass(ChatIO chatIo, UsersWordsService service, UserWordModel word,
            UserWordModel[] examList)
        {
            if (!word.Phrases.Any())
                return ExamResult.Impossible;

            var phrase = word.GetRandomExample();

            var replaced = phrase.TranslatedPhrase.Replace(phrase.TranslatedWord, "...");
            if (replaced == phrase.TranslatedPhrase)
                return ExamResult.Impossible;

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
                if (string.Compare(phrase.TranslatedWord, enter.Trim(), StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return ExamResult.Passed;
                }

                await chatIo.SendMessageAsync($"Origin phrase was \"{phrase.TranslatedPhrase}\"");
                return ExamResult.Failed;
            }
        }
    }
}