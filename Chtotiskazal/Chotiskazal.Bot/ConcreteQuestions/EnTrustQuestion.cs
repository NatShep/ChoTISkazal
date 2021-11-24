using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.ConcreteQuestions
{
    public class EnTrustQuestion : IQuestion
    {
        public bool NeedClearScreen => false;
        public string Name => "Eng trust";

        public async Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word,
            UserWordModel[] examList)
        {
            var msg = $"{QuestionHelper.QuestionPrefix}\r\n" +
                      $"\\=\\=\\=\\=\\=\\>   *{word.Word}*    \\<\\=\\=\\=\\=\\=\r\n" +
                      ""+chat.Texts.DoYouKnowTranslation+"";
            var id = Rand.Next();
            await chat.SendMarkdownMessageAsync(msg,
                new InlineKeyboardButton()
                {
                    CallbackData = id.ToString(),
                    Text =  chat.Texts.SeeTheTranslation 
                });
            while (true)
            {
                var update = await chat.WaitUserInputAsync();
                if(update.CallbackQuery?.Data== id.ToString())
                    break;
                var input = update.Message?.Text;
                if (!string.IsNullOrWhiteSpace(input))
                {
                    if (word.TextTranslations.Any(a =>
                        input.AreEqualIgnoreCase(a)))
                        return QuestionResult.Passed(chat.Texts);

                    await chat.SendMessageAsync(chat.Texts.ItIsNotRightTryAgain);
                }
            }

            await chat.SendMarkdownMessageAsync($"_{chat.Texts.TranslationIs}_ \r\n" +
                                        $"*\"{word.AllTranslationsAsSingleString}\"*\r\n\r\n" +
                                        $"{chat.Texts.DidYouGuess}",
                                            new[] {
                                                new[] {
                                                    new InlineKeyboardButton {
                                                        CallbackData = "1",
                                                        Text = chat.Texts.YesButton
                                                    },
                                                    new InlineKeyboardButton {
                                                        CallbackData = "0",
                                                        Text = chat.Texts.NoButton
                                                    }
                                                }
                                            });

            var choice = await chat.WaitInlineIntKeyboardInput();

            return choice == 1 ? 
                QuestionResult.Passed(chat.Texts.PassedOpenIHopeYouWereHonestMarkdown, chat.Texts.PassedHideousWellMarkdown ) 
                : QuestionResult.Failed(chat.Texts.FailedOpenButYouWereHonestMarkdown, chat.Texts.FailedHideousHonestyIsGoldMarkdown);
        }
    }
}