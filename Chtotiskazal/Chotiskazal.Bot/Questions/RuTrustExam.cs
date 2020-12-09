using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.Bll.Dto;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Words;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.Questions
{
    public class RuTrustExam : IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Ru trust";

        public async Task<ExamResult> Pass(ChatIO chatIo, UsersWordsService service, UserWordModel word, UserWordModel[] examList)
        {
            var msg = $"=====>   {word.TranslationAsList}    <=====\r\n" +
                      $"Do you know the translation?";
            var _ = chatIo.SendMessageAsync(msg,
                new InlineKeyboardButton()
                {
                    CallbackData = "1",
                    Text = "See the translation"
                });
            await chatIo.WaitInlineIntKeyboardInput();
            
            _= chatIo.SendMessageAsync($"Translation is \r\n" +
                                       $"{word.Word}\r\n" +
                                       $" Did you guess?",
                                new[]{
                                    new[]{
                                        new InlineKeyboardButton {
                                            CallbackData = "1",
                                            Text = "Yes"
                                        },
                                        new InlineKeyboardButton {
                                            CallbackData = "0",
                                            Text = "No"
                                        }
                                    }
                                }
                            );

            var choice = await chatIo.WaitInlineIntKeyboardInput();
            return choice == 1 
                ? ExamResult.Passed 
                : ExamResult.Failed;
        }
    }
}