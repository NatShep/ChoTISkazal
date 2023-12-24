using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions;

public class EngChooseWordInExampleScenario : IQuestionScenario {
    public QuestionInputType InputType => QuestionInputType.NeedsNoInput;
    public ScenarioWordTypeFit Fit => ScenarioWordTypeFit.OnlyWord;

    public async Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word,
        UserWordModel[] examList) {
        if (!word.Examples.Any())
            return QuestionResult.Impossible;

        var phrase = word.GetRandomExample();

        var replaced = phrase.OriginPhrase.Replace(phrase.OriginWord, "...");

        if (replaced == phrase.OriginPhrase)
            return QuestionResult.Impossible;

        var variants = examList
            .Where(p => !p.Examples.Select(e => e.TranslatedPhrase)
                .Any(t => t.AreEqualIgnoreCase(phrase.TranslatedPhrase)))
            .Select(e => e.Word)
            .Shuffle()
            .Take(5)
            .Append(phrase.OriginWord)
            .Shuffle()
            .ToArray();

        var _ = await chat.SendMarkdownMessageAsync(
            QuestionMarkups.TranslatesAsTemplate(
                phrase.TranslatedPhrase,
                chat.Texts.translatesAs,
                replaced,
                chat.Texts.ChooseMissingWord + ":")
            , InlineButtons.CreateVariants(variants));

        var choice = await chat.TryWaitInlineIntKeyboardInput();
        if (choice == null)
            return QuestionResult.RetryThisQuestion;

        if (variants[choice.Value].AreEqualIgnoreCase(word.Word))
            return QuestionResult.Passed(Markdown.Escaped(chat.Texts.Passed1),
                chat.Texts);

        return QuestionResult.Failed(
            Markdown.Escaped($"{chat.Texts.OriginWas}:").NewLine() +
            Markdown.Escaped($"\"{phrase.OriginPhrase}\"").ToSemiBold(),
            chat.Texts);
    }
}