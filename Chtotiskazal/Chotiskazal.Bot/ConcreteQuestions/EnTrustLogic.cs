using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.ConcreteQuestions;

public class EnTrustLogic : IQuestionLogic {
    public QuestionInputType InputType => QuestionInputType.NeedsNoInput;

    public async Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word,
        UserWordModel[] examList) {
        var msg = QuestionMarkups.TranslateTemplate(word.Word, chat.Texts.DoYouKnowTranslation);
        var id = Rand.Next();
        await chat.SendMarkdownMessageAsync(msg,
            new InlineKeyboardButton
            {
                CallbackData = id.ToString(),
                Text = chat.Texts.SeeTheTranslation
            });
        while (true) {
            var update = await chat.WaitUserInputAsync();
            if (update.CallbackQuery?.Data == id.ToString())
                break;
            var input = update.Message?.Text;
            if (string.IsNullOrWhiteSpace(input)) continue;
            if (word.TextTranslations.Any(a => input.AreEqualIgnoreCase(a)))
                return QuestionResult.Passed(chat.Texts);

            await chat.SendMessageAsync(chat.Texts.ItIsNotRightTryAgain);
        }

        await chat.SendMarkdownMessageAsync(Markdown.Escaped(chat.Texts.TranslationIs).ToItalic()
                                                .NewLine() +
                                            Markdown.Escaped($"\"{word.AllTranslationsAsSingleString}\"").ToSemiBold()
                                                .NewLine()
                                                .NewLine() +
                                            Markdown.Escaped(chat.Texts.DidYouGuess),
            new[]
            {
                new[]
                {
                    new InlineKeyboardButton
                    {
                        CallbackData = "1",
                        Text = chat.Texts.YesButton
                    },
                    new InlineKeyboardButton
                    {
                        CallbackData = "0",
                        Text = chat.Texts.NoButton
                    }
                }
            });

        var choice = await chat.WaitInlineIntKeyboardInput();

        return choice == 1
            ? QuestionResult.Passed(Markdown.Escaped(chat.Texts.PassedOpenIHopeYouWereHonest),
                Markdown.Escaped(chat.Texts.PassedHideousWell))
            : QuestionResult.Failed(Markdown.Escaped(chat.Texts.FailedOpenButYouWereHonest),
                Markdown.Escaped(chat.Texts.FailedHideousHonestyIsGold));
    }
}