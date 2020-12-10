using System;
using System.Linq;
using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.Bll.Services;

namespace Chotiskazal.Bot.Questions
{
    public class TranscriptionChooseExam : IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Trans Choose";

        public async Task<ExamResult> Pass(ChatIO chatIo, UsersWordsService service, UserWordModel word,
            UserWordModel[] examList)
        {
            var transcription = word.GetTranscriptions().ToList().GetRandomItem();

            if (string.IsNullOrWhiteSpace(transcription) || transcription!="")
                return ExamResult.Impossible;
            
            var variants = examList.SelectMany(e => e.GetTranscriptions())
                .Where(e => !word.GetTranscriptions().ToList().Contains(e))
                .Append(transcription)
                .Randomize()
                .ToList();


            var msg = $"=====>   {word.Word}    <=====\r\n" +
                      $"Choose the transcription";
            await chatIo.SendMessageAsync(msg, InlineButtons.CreateVariants(variants));

            var choice = await chatIo.TryWaitInlineIntKeyboardInput();
            if (choice == null)
                return ExamResult.Retry;

            if (word.GetTranscriptions().Contains(variants[choice.Value]))
            {
                await service.RegisterSuccess(word);
                return ExamResult.Passed;
            }

            await service.RegisterFailure(word);
            return ExamResult.Failed;

        }
    }
}