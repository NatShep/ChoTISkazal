using System;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.InterfaceLang;
using SayWhat.Bll;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.Questions
{
    public class EnTrustExam : IExam
    {
        public bool NeedClearScreen => false;
        public string Name => "Eng trust";

        public async Task<QuestionResult> Pass(ChatIO chatIo, UserWordModel word,
            UserWordModel[] examList)
        {
            var msg = $"=====>   {word.Word}    <=====\r\n" +
                      Texts.Current.DoYouKnowTranslation;
            var id = Rand.Next();
            var _ = chatIo.SendMessageAsync(msg,
                new InlineKeyboardButton()
                {
                    CallbackData = id.ToString(),
                    Text =  Texts.Current.SeeTheTranslation 
                });
            while (true)
            {
                var update = await chatIo.WaitUserInputAsync();
                if(update.CallbackQuery?.Data== id.ToString())
                    break;
                var input = update.Message?.Text;
                if (!string.IsNullOrWhiteSpace(input))
                {
                    if (word.AllTranslations.Any(a =>
                        input.AreEqualIgnoreCase(a)))
                        return QuestionResult.Passed;

                    await chatIo.SendMessageAsync(Texts.Current.ItIsNotRightTryAgain);
                }
            }

            _ = chatIo.SendMessageAsync($"{Texts.Current.TranslationIs} \r\n" +
                                        $"{word.TranslationAsList}\r\n" +
                                        $" {Texts.Current.DidYouGuess}",
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

            return choice == 1 ? 
                QuestionResult.PassedText(Texts.Current.PassedOpenIHopeYouWereHonest, Texts.Current.PassedHideousWell ) 
                : QuestionResult.FailedText(Texts.Current.FailedOpenButYouWereHonest, Texts.Current.FailedHideousHonestyIsGold);
        }
    }
}