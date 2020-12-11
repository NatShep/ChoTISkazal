using System;
using System.Linq;
using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.Questions
{
    public class TranscriptionChooseExam : IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Trans Choose";

        public async Task<ExamResult> Pass(ChatIO chatIo, UserWordModel word,
            UserWordModel[] examList)
        {
            var originalTranslation = word.Translations.GetRandomItem();

            if (originalTranslation==null || !originalTranslation.HasTranscription)
                return ExamResult.Impossible;
            
            var variants = examList
                .SelectMany(e => e.Translations)
                .Select(e=>e.Transcription)
                .Where(e => word.Translations.All(t => t.Transcription != e))
                .Append(originalTranslation.Transcription)
                .Randomize()
                .ToList();


            var msg = $"=====>   {word.Word}    <=====\r\n" +
                      $"Choose the transcription";
            await chatIo.SendMessageAsync(msg, InlineButtons.CreateVariants(variants));

            var choice = await chatIo.TryWaitInlineIntKeyboardInput();
            if (choice == null)
                return ExamResult.Retry;

            return word.Translations.Any(t=>t.Transcription== variants[choice.Value]) 
                ? ExamResult.Passed 
                : ExamResult.Failed;
        }
    }
}