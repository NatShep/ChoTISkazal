using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions;

public class EngChooseMultipleTranslationsScenario : IQuestionScenario {
    public QuestionInputType InputType => QuestionInputType.NeedsNoInput;

    public async Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word,
        UserWordModel[] examList) {
        var translations = string.Join(", ", word.TextTranslations.Shuffle().Take(3));

        var variants = examList
            .Where(e => e.AllTranslationsAsSingleString != word.AllTranslationsAsSingleString)
            .Select(e => string.Join(", ", e.TextTranslations.Shuffle().Take(3)))
            .Shuffle()
            .Take(5)
            .Append(translations)
            .ToList();

        var msg = QuestionMarkups.TranslateTemplate(word.Word, chat.Texts.ChooseTheTranslation);
        await chat.SendMarkdownMessageAsync(msg, InlineButtons.CreateVariants(variants));

        var choice = await chat.TryWaitInlineIntKeyboardInput();
        if (choice == null)
            return QuestionResult.RetryThisQuestion;

        var answer = variants[choice.Value].Split(",")
            .Select(e => e.Trim()).ToList();

        return !answer.Except(word.TextTranslations).Any()
            ? QuestionResult.Passed(chat.Texts)
            : QuestionResult.Failed(chat.Texts);
    }
}