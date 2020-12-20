using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.InterfaceLang;
using Chotiskazal.Bot.Questions;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.ConcreteQuestions
{
    public class IsItRightTranslationQuestion: IQuestion
    {
        public bool NeedClearScreen => false;
        public string Name => "Eng is it right transltion";

        public async Task<QuestionResult> Pass(ChatIO chatIo,  UserWordModel word,
            UserWordModel[] examList)
        {
            var translation = examList.SelectMany(e => e.TextTranslations)
                .Where(e => word.Translations.All(t => t.Word != e))
                .Randomize()
                .Take(1)
                .Union(word.TextTranslations)
                .ToList()
                .GetRandomItem();
            
            var msg = $"'{word.Word}' {Texts.Current.translatesAs} '{translation}'.\r\n"+
                             $"{Texts.Current.IsItRightTranslation}";

            _ = chatIo.SendMessageAsync(msg,
                new[] {
                    new[] {
                        new InlineKeyboardButton {
                            CallbackData = "1",
                            Text = Texts.Current.YesButton
                        },
                        new InlineKeyboardButton {
                            CallbackData = "0",
                            Text = Texts.Current.NoButton
                        }
                    }
                });

            var choice = await chatIo.WaitInlineIntKeyboardInput();
            if  (
                choice == 1 &&  word.TextTranslations.Contains(translation) ||
                choice == 0 && !word.TextTranslations.Contains(translation)
                )
            {
                return QuestionResult.Passed;
            }
            else
            {
                return QuestionResult.FailedText($"{Texts.Current.Mistaken}. '{word.Word}' {Texts.Current.translatesAs} '{word.TextTranslations.FirstOrDefault()}' ");
            }
        }
    }
}