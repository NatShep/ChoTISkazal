using System.Linq;
using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.Bll.Dto;
using SayWhat.Bll.Services;

namespace Chotiskazal.Bot.Questions
{
    public class RuChooseExam: IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "RuChoose";

        public async Task<ExamResult> Pass(ChatIO chatIo, UsersWordsService service, UserWordModel word, UserWordModel[] examList)
        {
            var variants = examList.Randomize().Select(e => e.Word).ToArray();

            var msg = $"=====>   {word.TranlationAsList}    <=====\r\nChoose the translation";
            await chatIo.SendMessageAsync(msg, InlineButtons.CreateVariants(variants));
            
            var choice = await chatIo.TryWaitInlineIntKeyboardInputAsync();
            if (choice == null)
                return ExamResult.Retry;
            
            if (variants[choice.Value] == word.Word)
            {
                await service.RegisterSuccess(word);
                return ExamResult.Passed;
            }
            await service.RegisterFailure(word);

            return ExamResult.Failed;
        }
    }
}