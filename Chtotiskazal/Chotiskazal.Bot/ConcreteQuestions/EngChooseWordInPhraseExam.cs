using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chotiskazal.Bot.InterfaceLang;
using SayWhat.Bll;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.Questions
{
    public class EngChooseWordInPhraseExam : IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Eng Choose word in phrase";

        public async Task<QuestionResult> Pass(ChatIO chatIo, UserWordModel word,
            UserWordModel[] examList)
        {
            if (!word.Examples.Any())
                return QuestionResult.Impossible;

            var phrase = word.GetRandomExample();

            var replaced = phrase.OriginPhrase.Replace(phrase.OriginWord, "...");

            if (replaced == phrase.OriginPhrase)
                return QuestionResult.Impossible;
            
            var sb = new StringBuilder();
            sb.AppendLine($"\"{phrase.TranslatedPhrase}\"");
            sb.AppendLine();
            sb.AppendLine($" {Texts.Current.translatesAs} ");
            sb.AppendLine();
            sb.AppendLine($"\"{replaced}\"");
            sb.AppendLine($"{Texts.Current.ChooseMissingWord}...");


            var variants = examList
                .Where(p => !p.Examples.Select(e=>e.TranslatedPhrase)
                    .Any(t=>t.AreEqualIgnoreCase(phrase.TranslatedPhrase)))
                .Select(e => e.Word)
                .Randomize()
                .Take(5)
                .Append(phrase.OriginWord)
                .Randomize()
                .ToArray();

            var _ = chatIo.SendMessageAsync(sb.ToString(), InlineButtons.CreateVariants(variants));

            var choice = await chatIo.TryWaitInlineIntKeyboardInput();
            if (choice == null)
                return QuestionResult.Retry;

            if (variants[choice.Value].AreEqualIgnoreCase(word.Word))
                return QuestionResult.PassedText(Texts.Current.Passed1);
            
            return QuestionResult.FailedText($"{Texts.Current.OriginWas}: \"{phrase.OriginPhrase}\"");
        }
    }
}