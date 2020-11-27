using System.Linq;
using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.Bll.Dto;
using SayWhat.Bll.Services;

namespace Chotiskazal.Bot.Questions
{
    public class EngChooseExam:IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Eng Choose";

        public async Task<ExamResult> Pass(ChatIO chatIo, UsersWordsService service, UserWordModel word,
            UserWordModel[] examList)
        {
            var variants = examList.Randomize().SelectMany(e => e.GetTranslations()).ToArray();

            var msg = $"=====>   {word.Word}    <=====\r\n" +
                      $"Choose the translation";
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