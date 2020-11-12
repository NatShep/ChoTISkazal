using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.ConsoleTesting.Services;
using Chotiskazal.DAL;
using Chotiskazal.DAL.Services;

namespace Chotiskazal.Bot.Questions
{
    public class RuChooseExam: IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "RuChoose";

        public async Task<ExamResult> Pass(ChatIO chatIo, ExamService service, UserWordForLearning word, UserWordForLearning[] examList)
        {
            var variants = examList.Randomize().Select(e => e.EnWord).ToArray();

            var msg = $"=====>   {word.UserTranslations}    <=====\r\nChoose the translation";
            await chatIo.SendMessage(msg, InlineButtons.CreateVariants(variants));
            
            var choice = await chatIo.TryWaitInlineIntKeyboardInput();
            if (choice == null)
                return ExamResult.Retry;
            
            if (variants[choice.Value] == word.EnWord)
            {
                await service.RegistrateSuccessAsync(word);
                return ExamResult.Passed;
            }
            await service.RegistrateFailureAsync(word);

            return ExamResult.Failed;
        }
    }
}