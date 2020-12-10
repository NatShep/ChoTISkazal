using System.Linq;
using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.Bll.Services;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.Questions
{
    public class IsItRightTranslationExam: IExam
    {
        public bool NeedClearScreen => false;
        public string Name => "Eng trust";

        public async Task<ExamResult> Pass(ChatIO chatIo, UsersWordsService service, UserWordModel word,
            UserWordModel[] examList)
        {
            var translation = examList.SelectMany(e => e.GetTranslations())
                .Where(e => !word.GetTranslations().ToList().Contains(e))
                .Randomize()
                .Take(1)
                .Union(word.GetTranslations())
                .ToList()
                .GetRandomItem();

       /*     var translation = word.GetTranslations()
                .Append(examList.SelectMany(e => e.GetTranslations())
                    .Where(e => !word.GetTranslations().ToList().Contains(e))
                    .ToList()
                    .GetRandomItem())
                .ToList()
                .GetRandomItem();
                
          */
            
            var msg = $"{word.Word.ToUpper()} translates as {translation.ToUpper()}.\r\n"+
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

            if  ((choice == 1 && word.GetTranslations().Contains(translation)) ||
                (choice ==0 && !word.GetTranslations().Contains(translation)))
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