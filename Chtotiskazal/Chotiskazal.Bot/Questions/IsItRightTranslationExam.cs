using System.Linq;
using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.Questions
{
    public class IsItRightTranslationExam: IExam
    {
        public bool NeedClearScreen => false;
        public string Name => "Eng trust";

        public async Task<QuestionResult> Pass(ChatIO chatIo,  UserWordModel word,
            UserWordModel[] examList)
        {
            var translation = examList.SelectMany(e => e.AllTranslations)
                .Where(e => word.Translations.All(t => t.Word != e))
                .Randomize()
                .Take(1)
                .Union(word.AllTranslations)
                .ToList()
                .GetRandomItem();
            
            var msg = $"'{word.Word}' translates as '{translation}'.\r\n"+
                             $"Is it right translation?";

            _ = chatIo.SendMessageAsync(msg,
                new[] {
                    new[] {
                        new InlineKeyboardButton {
                            CallbackData = "1",
                            Text = "Yes"
                        },
                        new InlineKeyboardButton {
                            CallbackData = "0",
                            Text = "No"
                        }
                    }
                });

            var choice = await chatIo.WaitInlineIntKeyboardInput();

            if  (
                choice == 1 &&  word.AllTranslations.Contains(translation) ||
                choice == 0 && !word.AllTranslations.Contains(translation)
                )
            {
                return QuestionResult.Passed;
            }
            else
            {
                return QuestionResult.FailedText($"Mistaken. '{word.Word}' translates as '{translation}' ");
            }
        }

    }
}