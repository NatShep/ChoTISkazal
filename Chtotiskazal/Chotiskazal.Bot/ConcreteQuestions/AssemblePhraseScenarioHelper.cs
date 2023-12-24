using System;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL;

namespace Chotiskazal.Bot.ConcreteQuestions;

public static class AssemblePhraseScenarioHelper {
    public static async Task<QuestionResult> AssemblePhrase(ChatRoom chat,
        string enPhrase,
        string ruTranslation = null) {
        var shuffled = ShuffleWordsOrNull(enPhrase);
        if (shuffled == null)
            return QuestionResult.Impossible;

        var msg = ruTranslation == null
            ? chat.Texts.WordsInPhraseAreShuffledWriteThemInOrder.AddEscaped(":")
                .NewLine() + Markdown.Escaped(shuffled).ToSemiBold()
            : chat.Texts.WordsInPhraseWithClueAreShuffledWriteThemInOrder(shuffled, ruTranslation);

        return await HandleAssemblePhraseUserInput(chat, msg, originPhrase: enPhrase);
    }

    private static string ShuffleWordsOrNull(string phrase) {
        while (true) {
            var wordsInExample = phrase.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (wordsInExample.Length < 2)
                return null;

            var shuffled = string.Join(" ", wordsInExample.Shuffle());
            if (shuffled != phrase)
                return shuffled;
        }
    }

    private static async Task<QuestionResult> HandleAssemblePhraseUserInput(ChatRoom chat, Markdown questionText, string originPhrase) {
        var (result, entry) = await QuestionScenarioHelper.GetEnglishUserInputOrIDontKnow(chat,questionText);

        if (result == OptionalUserInputResult.IDontKnow)
            return Failed();
        if (result == OptionalUserInputResult.NotAnInput)
            return QuestionResult.RetryThisQuestion;

        var closeness = originPhrase.CheckCloseness(entry.Trim());

        switch (closeness) {
            case StringsCompareResult.Equal:
                return QuestionResult.Passed(chat.Texts);
            case StringsCompareResult.SmallMistakes:
            case StringsCompareResult.BigMistakes:
                await chat.SendMessageAsync(chat.Texts.RetryAlmostRightWithTypo);
                return QuestionResult.RetryThisQuestion;
            case StringsCompareResult.NotEqual:
            default:
                return Failed();
        }

        QuestionResult Failed() => QuestionResult.Failed(
            chat.Texts.FailedOriginPhraseWas2 + Markdown.Escaped(":")
                .NewLine()
                .AddMarkdown($"\"{originPhrase}\"".ToSemiBoldMarkdown()),
            chat.Texts);
    }
}