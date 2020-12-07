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
            var variants = examList.SelectMany(e => e.GetTranscription())
                .Where(e => !word.GetTranscription().ToList().Contains(e))
                .Union(word.GetTranscription())
                .Randomize()
                .ToList();


            var msg = $"=====>   {word.Word}    <=====\r\n" +
                      $"Choose the transcription";
            await chatIo.SendMessageAsync(msg, InlineButtons.CreateVariants(variants));

            var choice = await chatIo.TryWaitInlineIntKeyboardInput();
            if (choice == null)
                return ExamResult.Retry;

            if (word.GetTranscription().Contains(variants[choice.Value]))
            {
                await service.RegisterSuccess(word);
                return ExamResult.Passed;
            }

            await service.RegisterFailure(word);
            return ExamResult.Failed;

        }
    }
}