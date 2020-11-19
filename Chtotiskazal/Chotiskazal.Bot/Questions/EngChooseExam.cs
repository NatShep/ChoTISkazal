using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Services;
using SayWhat.Bll;
using SayWhat.Bll.Dto;

namespace Chotiskazal.Bot.Questions
{
    public class EngChooseExam:IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Eng Choose";

        public async Task<ExamResult> Pass(ChatIO chatIo, ExamService service, UserWordModel word,
            UserWordModel[] examList)
        {
            
            var variants = examList.Randomize().SelectMany(e => e.GetTranslations()).ToArray();

            var msg = $"=====>   {word.Word}    <=====\r\nChoose the translation";
            await chatIo.SendMessageAsync(msg, InlineButtons.CreateVariants(variants));

            var choice = await chatIo.TryWaitInlineIntKeyboardInputAsync();
            if (choice == null)
                return ExamResult.Retry;

            if (word.GetTranslations().Contains(variants[choice.Value]))
            {
                await service.RegisterSuccessAsync(word);
                return ExamResult.Passed;
            }

            await service.RegisterFailureAsync(word);
            return ExamResult.Failed;
        }
    }
}