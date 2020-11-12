using System.Threading.Tasks;
using Chotiskazal.ConsoleTesting.Services;
using Chotiskazal.DAL;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.Questions
{
    public class RuTrustExam : IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Ru trust";

        public async Task<ExamResult> Pass(ChatIO chatIo, ExamService service, UserWordForLearning word, UserWordForLearning[] examList)
        {
            var msg = $"=====>   {word.UserTranslations}    <=====\r\nDo you know the translation?";
            var _ = chatIo.SendMessageAsync(msg,
                new InlineKeyboardButton()
                {
                    CallbackData = "1",
                    Text = "See the translation"
                });
            await chatIo.WaitInlineIntKeyboardInput();
            
            _= chatIo.SendMessageAsync("Translation is \r\n" + word.EnWord + "\r\n Did you guess?",
                
                new InlineKeyboardButton
                {
                    CallbackData = "1",
                    Text = "Yes"
                },
                new InlineKeyboardButton{
                    CallbackData = "0",
                    Text = "No"
                });
            
            var choice = await chatIo.WaitInlineIntKeyboardInput();

            if (choice == 1)
            {
                await  service.RegistrateSuccessAsync(word);
                return ExamResult.Passed;
            }
            else
            {
               await service.RegistrateFailureAsync(word);
                return ExamResult.Failed;
            }
        }
    }
}