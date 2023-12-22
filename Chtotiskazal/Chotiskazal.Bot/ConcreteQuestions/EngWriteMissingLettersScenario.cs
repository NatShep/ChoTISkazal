using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions;

public class EngWriteMissingLettersScenario : IQuestionScenario {
    private readonly StarredHardness _hardness;

    public EngWriteMissingLettersScenario(StarredHardness hardness) {
        _hardness = hardness;
    }

    public QuestionInputType InputType => QuestionInputType.NeedsRuInput;

    public async Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList) {
        var engWord = word.Word;

        var ruWord = word.RuTranslations.Where(t => !t.Word.Contains(' ') && t.Word.Length > 2).GetRandomItemOrNull()
            ?.Word;
        if (ruWord == null)
            return QuestionResult.Impossible;

        string starred = ruWord.GetWithStarredBody(_hardness, out var body);

        var msg = QuestionMarkups.TranslatesAsTemplate(
            engWord,
            chat.Texts.translatesAs,
            starred,
            chat.Texts.WriteMissingLettersOrTheWholeWord);

        var (result, entry) = await QuestionScenarioHelper.GetRussianUserInputOrIDontKnow(chat, msg);

        if (result == OptionalUserInputResult.IDontKnow)
            return QuestionResult.Failed(Markdown.Empty, Markdown.Empty);
        if (result == OptionalUserInputResult.NotAnInput)
            return QuestionResult.RetryThisQuestion;

        var bodyCloseness = entry.CheckCloseness(body);
        var wordCloseness = entry.CheckCloseness(ruWord);
        var closeness = bodyCloseness > wordCloseness ? bodyCloseness : wordCloseness;

        switch (closeness) {
            case StringsCompareResult.Equal:
                return QuestionResult.Passed(chat.Texts);
            case StringsCompareResult.SmallMistakes:
                await chat.SendMarkdownMessageAsync(chat.Texts.YouHaveATypoLetsTryAgain(ruWord));
                return QuestionResult.RetryThisQuestion;
            case StringsCompareResult.BigMistakes:
                return QuestionResult.Failed(chat.Texts.FailedMistaken(ruWord), chat.Texts);
        }

        return QuestionResult.Failed(chat.Texts.FailedMistaken(ruWord), chat.Texts);
    }
}