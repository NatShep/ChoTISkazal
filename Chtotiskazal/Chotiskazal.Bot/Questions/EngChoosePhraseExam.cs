using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Services;
using SayWhat.Bll;
using SayWhat.Bll.Dto;

namespace Chotiskazal.Bot.Questions
{
    public class EngChoosePhraseExam : IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Eng Choose Phrase";

        public async Task<ExamResult> Pass(
            ChatIO chatIo, 
            ExamService service, 
            UserWordModel word, 
            UserWordModel[] examList)
        {
            if (!word.HasAnyPhrases)
                return ExamResult.Impossible;
            
            var targetPhrase = word.GetRandomExample();

            var otherExamples = examList
                .SelectMany(e => e.Phrases)
                .Where(p => p != targetPhrase)
                .Take(8).ToArray();

            if(!otherExamples.Any())
                return ExamResult.Impossible;

            var variants = otherExamples
                .Append(targetPhrase)
                .Randomize()
                .Select(e => e.PhraseTranslation)
                .ToArray();
            
            var msg = $"=====>   {targetPhrase.OriginPhrase}    <=====\r\nChoose the translation";
            await chatIo.SendMessageAsync(msg, InlineButtons.CreateVariants(variants));
            
            var choice = await chatIo.TryWaitInlineIntKeyboardInputAsync();
            if (choice == null)
                return ExamResult.Retry;
            
            if (variants[choice.Value] == targetPhrase.PhraseTranslation)
            {
                await service.RegisterSuccessAsync(word);
                return ExamResult.Passed;
            }
            await service.RegisterFailureAsync(word);
            return ExamResult.Failed;
        }
    }
}