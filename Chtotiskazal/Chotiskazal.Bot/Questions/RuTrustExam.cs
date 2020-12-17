using System;
using System.Linq;
using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.Questions
{
    public class RuTrustExam : IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Ru trust";

        public async Task<QuestionResult> Pass(ChatIO chatIo, UserWordModel word, UserWordModel[] examList)
        {
            var msg = $"=====>   {word.TranslationAsList}    <=====\r\n" +
                      $"Do you know the translation?";
            var id = Rand.Next();
            
            var _ = chatIo.SendMessageAsync(msg,
                new InlineKeyboardButton()
                {
                    CallbackData = id.ToString(),
                    Text = "See the translation"
                });
            while (true)
            {
                var update = await chatIo.WaitUserInputAsync();
                if (update.CallbackQuery?.Data == id.ToString())
                    break;
                var input = update.Message?.Text;
                if (!string.IsNullOrWhiteSpace(input))
                {
                    if (word.Word.AreEqualIgnoreCase(input))
                        return QuestionResult.Passed;
                    await chatIo.SendMessageAsync("No. It is not right. Try again");
                }
            }

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
                ? QuestionResult.PassedText("Good. I hope you were honest", "Good") 
                : QuestionResult.FailedText("But you were honest...", "Honesty is gold");
        }
    }
}