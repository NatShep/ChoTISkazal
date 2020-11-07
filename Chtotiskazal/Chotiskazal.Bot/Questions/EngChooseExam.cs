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

        public async Task<ExamResult> Pass(Chat chat, ExamService service, UserWordForLearning word, UserWordForLearning[] examList)
        {
            var variants = examList.Randomize().Select(e => e.UserTranslations).ToArray();

            var msg = $"=====>   {word.EnWord}    <=====\r\nChoose the translation";
            await chat.SendMessage(msg, InlineButtons.CreateVariants(variants));
           
                chat = chat as Chat;
                var choice = await chat.TryWaitInlineIntKeyboardInput();
                if (choice == null)
                    return ExamResult.Retry;
            

            if (variants[choice.Value] == word.UserTranslations)
            {
                service.RegistrateSuccess(word);
                return ExamResult.Passed;
            }
            service.RegistrateFailure(word);
            return ExamResult.Failed;

        }
    }
}