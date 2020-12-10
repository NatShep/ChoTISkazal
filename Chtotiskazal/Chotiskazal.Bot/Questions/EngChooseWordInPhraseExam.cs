using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.Bll.Dto;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Examples;

namespace Chotiskazal.Bot.Questions
{
    public class EngChooseWordInPhraseExam : IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Eng Choose word in phrase";

        public async Task<ExamResult> Pass(ChatIO chatIo, UsersWordsService service, UserWordModel word,
            UserWordModel[] examList)
        {
            if (!word.Phrases.Any())
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
                .Where(p => !p.Phrases.Select(e=>e.TranslatedPhrase.ToLower()).Contains(phrase.TranslatedPhrase.ToLower()))
                .Select(e => e.Word)
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
                await service.RegisterSuccess(word);
                return ExamResult.Passed;
            }

            await chatIo.SendMessageAsync($"Origin was: \"{phrase.OriginPhrase}\"");
            await service.RegisterFailure(word);
            return ExamResult.Failed;
        }
    }
}