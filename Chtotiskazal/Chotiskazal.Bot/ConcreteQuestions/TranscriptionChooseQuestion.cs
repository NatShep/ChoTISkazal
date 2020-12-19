using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.InterfaceLang;
using Chotiskazal.Bot.Questions;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions
{
    public class TranscriptionChooseQuestion : IQuestion
    {
        public bool NeedClearScreen => false;

        public string Name => "Trans Choose";

        public async Task<QuestionResult> Pass(ChatIO chatIo, UserWordModel word,
            UserWordModel[] examList)
        {
            var originalTranslation = word.Translations.GetRandomItem();

            if (originalTranslation==null || !originalTranslation.HasTranscription)
                return QuestionResult.Impossible;
            
            var variants = examList
                .SelectMany(e => e.Translations)
                .Select(e=>e.Transcription)
                .Where(e => word.Translations.All(t => t.Transcription != e))
                .Distinct()
                .Randomize()
                .Take(5)
                .Append(originalTranslation.Transcription)
                .Where(w=>w!=null)
                .Randomize()
                .ToList();
            
            if (variants.Count == 0)
                return QuestionResult.Impossible;

            var msg = $"=====>   {word.Word}    <=====\r\n" +
                      Texts.Current.ChooseTheTranscription;
            await chatIo.SendMessageAsync(msg, InlineButtons.CreateVariants(variants));

            var choice = await chatIo.TryWaitInlineIntKeyboardInput();
            if (choice == null)
                return QuestionResult.Retry;

            return word.Translations.Any(t=>t.Transcription== variants[choice.Value]) 
                ? QuestionResult.Passed 
                : QuestionResult.Failed;
        }
    }
}