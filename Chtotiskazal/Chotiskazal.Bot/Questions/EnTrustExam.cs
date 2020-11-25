using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.Bll.Dto;
using SayWhat.Bll.Services;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.Questions
{
    public class EnTrustExam : IExam
    {
        public bool NeedClearScreen => false;
        public string Name => "Eng trust";

        public async Task<ExamResult> Pass(ChatIO chatIo, UsersWordsService service, UserWordModel word,
            UserWordModel[] examList)
        {
            var msg = $"=====>   {word.Word}    <=====\r\n" +
                      $"Do you know the translation?";
            var _ = chatIo.SendMessageAsync(msg,
                new InlineKeyboardButton()
                {
                    CallbackData = "1",
                    Text = "See the translation"
                });
            await chatIo.WaitInlineIntKeyboardInput();

            _ = chatIo.SendMessageAsync($"Translation is \r\n" +
                                        $"{word.TranlationAsList}\r\n" +
                                        $" Did you guess?",

                new InlineKeyboardButton
                {
                    CallbackData = "1",
                    Text = "Yes"
                },
                new InlineKeyboardButton
                {
                    CallbackData = "0",
                    Text = "No"
                });

            var choice = await chatIo.WaitInlineIntKeyboardInput();

            if (choice == 1)
            {
                await service.RegisterSuccess(word);
                return ExamResult.Passed;
            }
            else
            {
                await service.RegisterFailure(word);
                return ExamResult.Failed;
            }
        }
    }
}