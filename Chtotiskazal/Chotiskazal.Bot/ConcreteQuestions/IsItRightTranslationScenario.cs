using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions;

public class IsItRightTranslationScenario : IQuestionScenario {
    public QuestionInputType InputType => QuestionInputType.NeedsNoInput;
    public ScenarioWordTypeFit Fit => ScenarioWordTypeFit.WordAndPhrase;

    public async Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList) {
        var translation = examList.SelectMany(e => e.TextTranslations)
            .Where(e => word.RuTranslations.All(t => t.Word != e))
            .Shuffle()
            .Take(1)
            .Union(word.TextTranslations)
            .ToList()
            .GetRandomItemOrNull();

        var msg = QuestionMarkups.TranslatesAsTemplate(
            word.Word,
            chat.Texts.translatesAs,
            translation,
            chat.Texts.IsItRightTranslation);

        await chat.SendMarkdownMessageAsync(msg, new[] { InlineButtons.YesNo(chat.Texts) });

        var choice = await chat.WaitInlineIntKeyboardInput();
        if (
            choice == 1 && word.TextTranslations.Contains(translation) ||
            choice == 0 && !word.TextTranslations.Contains(translation)
        ) {
            return QuestionResult.Passed(chat.Texts);
        }
        else {
            return QuestionResult.Failed(
                Markdown.Escaped($"{chat.Texts.Mistaken}.").NewLine() +
                Markdown.Escaped($"\"{word.Word}\" {chat.Texts.translatesAs} ") +
                Markdown.Escaped($"\"{word.TextTranslations.FirstOrDefault()}\" ").ToSemiBold(),
                chat.Texts);
        }
    }
}