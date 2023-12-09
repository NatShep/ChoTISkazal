using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions;

public class EngWriteMissingLettersQuestion : IQuestion {
    private readonly StarredHardness _hardness;
    public EngWriteMissingLettersQuestion(StarredHardness hardness) {
        _hardness = hardness;

    }
    public bool NeedClearScreen => false;
    public string Name => "Eng write mising";
    public double PassScore => 1.3;
    public double FailScore => 0.52;

    public async Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList) {
        var engWord = word.Word;

        var ruWord = word.RuTranslations.Where(t => !t.Word.Contains(' ') && t.Word.Length>2).GetRandomItemOrNull()?.Word;
        if (ruWord == null)
            return QuestionResult.Impossible;

        string starred = ruWord.GetWithStarredBody(_hardness, out var body);

        var msg = QuestionMarkups.TranslatesAsTemplate(
            engWord,
            chat.Texts.translatesAs,
            starred,
            chat.Texts.WriteMissingLettersOrTheWholeWord);

        await chat.SendMarkdownMessageAsync(msg);
        var entry = await chat.WaitUserTextInputAsync();

        if (string.IsNullOrEmpty(entry))
            return QuestionResult.RetryThisQuestion;

        if (!entry.IsRussian()) {
            await chat.SendMessageAsync(chat.Texts.RussianInputExpected);
            return QuestionResult.RetryThisQuestion;
        }
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
                return QuestionResult.Failed(chat.Texts.FailedMistaken(ruWord),chat.Texts);
        }
        return QuestionResult.Failed(chat.Texts.FailedMistaken(ruWord),chat.Texts);

    }
}