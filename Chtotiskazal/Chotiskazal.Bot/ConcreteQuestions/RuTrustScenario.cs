using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions;

public class RuTrustScenario : IQuestionScenario {
    public QuestionInputType InputType => QuestionInputType.NeedsNoInput;
    public ScenarioWordTypeFit Fit => ScenarioWordTypeFit.WordAndPhrase;

    public async Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList) {
        var msg = QuestionMarkups.TranslateTemplate(word.AllTranslationsAsSingleString,
            chat.Texts.DoYouKnowTranslation);
        var id = Rand.Next();

        await chat.SendMarkdownMessageAsync(msg,
            InlineButtons.Button(chat.Texts.ShowTheTranslationButton, id.ToString()));
        while (true) {
            var update = await chat.WaitUserInputAsync();
            if (update.CallbackQuery?.Data == id.ToString())
                break;
            var input = update.Message?.Text;
            if (!string.IsNullOrWhiteSpace(input)) {
                if (word.Word.AreEqualIgnoreCase(input))
                    return QuestionResult.Passed(chat.Texts);
                await chat.SendMessageAsync(chat.Texts.ItIsNotRightTryAgain);
            }
        }

        await chat.SendMarkdownMessageAsync(Markdown.Escaped(chat.Texts.TranslationIs).ToItalic()
                                                .NewLine() +
                                            Markdown.Escaped($"\"{word.Word}\"").ToSemiBold()
                                                .NewLine()
                                                .NewLine()
                                                .AddEscaped(chat.Texts.DidYouGuess),
            new[] { InlineButtons.YesNo(chat.Texts) }
        );

        var choice = await chat.WaitInlineIntKeyboardInput();
        return choice == 1
            ? QuestionResult.Passed(
                Markdown.Escaped(chat.Texts.PassedOpenIHopeYouWereHonest),
                Markdown.Escaped(chat.Texts.PassedHideousWell2))
            : QuestionResult.Failed(
                Markdown.Escaped(chat.Texts.FailedOpenButYouWereHonest),
                Markdown.Escaped(chat.Texts.FailedHideousHonestyIsGold));
    }
}