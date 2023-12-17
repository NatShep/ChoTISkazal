using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions;

public class RuWriteMissingLettersLogic : IQuestionLogic {
    private readonly StarredHardness _hardness;

    public RuWriteMissingLettersLogic(StarredHardness hardness) {
        _hardness = hardness;
    }

    public async Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList) {
        var engWord = word.Word;

        if (engWord.Length < 2)
            return QuestionResult.Impossible;

        var ruWord = word.RuTranslations.Where(t => !t.Word.Contains(' ')).GetRandomItemOrNull()?.Word;
        if (ruWord == null)
            return QuestionResult.Impossible;

        string starred = engWord.GetWithStarredBody(_hardness, out var body);

        var msg = QuestionMarkups.TranslatesAsTemplate(
            ruWord,
            chat.Texts.translatesAs,
            starred,
            chat.Texts.WriteMissingLettersOrTheWholeWord);

        await chat.SendMarkdownMessageAsync(msg);
        var entry = await chat.WaitUserTextInputAsync();

        if (string.IsNullOrEmpty(entry))
            return QuestionResult.RetryThisQuestion;

        if (entry.IsRussian()) {
            await chat.SendMessageAsync(chat.Texts.EnglishInputExpected);
            return QuestionResult.RetryThisQuestion;
        }

        var bodyCloseness = entry.CheckCloseness(body);
        var wordCloseness = entry.CheckCloseness(word.Word);
        var closeness = bodyCloseness > wordCloseness ? bodyCloseness : wordCloseness;

        switch (closeness) {
            case StringsCompareResult.Equal:
                return QuestionResult.Passed(chat.Texts);
            case StringsCompareResult.SmallMistakes:
                await chat.SendMarkdownMessageAsync(chat.Texts.YouHaveATypoLetsTryAgain(engWord));
                return QuestionResult.RetryThisQuestion;
            case StringsCompareResult.BigMistakes:
                return QuestionResult.Failed(chat.Texts.FailedMistaken(engWord), chat.Texts);
        }

        return QuestionResult.Failed(chat.Texts.FailedMistaken(engWord), chat.Texts);
    }
}