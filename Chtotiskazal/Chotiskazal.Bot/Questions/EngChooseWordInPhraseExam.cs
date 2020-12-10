using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.Questions
{
    public class EngChooseWordInPhraseExam : IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Eng Choose word in phrase";

        public async Task<ExamResult> Pass(ChatIO chatIo, UserWordModel word,
            UserWordModel[] examList)
        {
            if (!word.Examples.Any())
                return ExamResult.Impossible;

            var phrase = word.GetRandomExample();

            var replaced = phrase.OriginPhrase.Replace(phrase.OriginWord, "...");

            if (replaced == phrase.OriginPhrase)
                return ExamResult.Impossible;

            var sb = new StringBuilder();
            sb.AppendLine($"\"{phrase.TranslatedPhrase}\"");
            sb.AppendLine();
            sb.AppendLine($" translated as ");
            sb.AppendLine();
            sb.AppendLine($"\"{replaced}\"");
            sb.AppendLine($"Choose missing word...");

            var variants = examList
                .Where(e => !e.Examples.Select(p => p.TranslatedPhrase).ToList().Contains(phrase.TranslatedPhrase))
                .Select(w => w.Word)
                .Randomize()
                .Take(5)
                .Append(phrase.OriginWord)
                .Randomize()
                .ToArray();

            var _ = chatIo.SendMessageAsync(sb.ToString(), InlineButtons.CreateVariants(variants));

            var choice = await chatIo.TryWaitInlineIntKeyboardInput();
            if (choice == null)
                return ExamResult.Retry;

            if (string.Equals(variants[choice.Value], word.Word, StringComparison.InvariantCultureIgnoreCase))
            {
                return ExamResult.Passed;
            }

            await chatIo.SendMessageAsync($"Origin was: \"{phrase.OriginPhrase}\"");
            return ExamResult.Failed;
        }
    }
}