using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.ConsoleTesting.Services;
using Chotiskazal.DAL;
using Chotiskazal.DAL.Services;

namespace Chotiskazal.Bot.Questions
{
    public class EngChooseExam:IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Eng Choose";

        public async Task<ExamResult> Pass(ChatIO chatIo, ExamService service, UserWordForLearning word, UserWordForLearning[] examList)
        {
            var variants = examList.Randomize().Select(e => e.UserTranslations).ToArray();

            var msg = $"=====>   {word.EnWord}    <=====\r\nChoose the translation";
            await chatIo.SendMessageAsync(msg, InlineButtons.CreateVariants(variants));
           
                chatIo = chatIo as ChatIO;
                var choice = await chatIo.TryWaitInlineIntKeyboardInputAsync();
                if (choice == null)
                    return ExamResult.Retry;
            

            if (variants[choice.Value] == word.UserTranslations)
            {
                await service.RegistrateSuccessAsync(word);
                return ExamResult.Passed;
            }
            await service.RegistrateFailureAsync(word);
            return ExamResult.Failed;

        }
    }
}