using System.Linq;
using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.Bll.Services;

namespace Chotiskazal.Bot.Questions
{
    public class RuChooseByTranscriptionExam:IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Ru Choose By Transcription";

        public async Task<ExamResult> Pass(ChatIO chatIo, UsersWordsService service, UserWordModel word,
            UserWordModel[] examList)
        {
            var translation = word.GetUserWordTranslations().ToList().GetRandomItem();
            
            if (string.IsNullOrWhiteSpace(translation.Transcription) || translation.Transcription!="")
                return ExamResult.Impossible;
            
            var variants = examList.Where(e=>!e.GetTranscriptions().ToList().Contains(translation.Transcription))
                .SelectMany(e => e.GetTranslations())
                .Take(5)
                .Append(translation.Word)
                .Randomize()
                .ToList();


            var msg = $"=====>   {translation.Transcription}    <=====\r\n" +
                      $"Choose which word has this transcription";
            await chatIo.SendMessageAsync(msg, InlineButtons.CreateVariants(variants));

            var choice = await chatIo.TryWaitInlineIntKeyboardInput();
            if (choice == null)
                return ExamResult.Retry;

            if (word.GetTranslations().Contains(variants[choice.Value]))
            {
                await service.RegisterSuccess(word);
                return ExamResult.Passed;
            }

            await service.RegisterFailure(word);
            return ExamResult.Failed;

        }
        
    }
}