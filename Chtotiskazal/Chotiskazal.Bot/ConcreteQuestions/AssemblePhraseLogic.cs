using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions;

public class AssemblePhraseLogic : IQuestionLogic {
    public QuestionInputType InputType => QuestionInputType.NeedsEnInput;

    public async Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList) {
        if (!word.HasAnyExamples)
            return QuestionResult.Impossible;

        var targetPhrase = word.GetRandomExample();

        string shuffled;
        while (true) {
            var wordsInExample = targetPhrase.SplitWordsOfPhrase;

            if (wordsInExample.Length < 2)
                return QuestionResult.Impossible;

            shuffled = string.Join(" ", wordsInExample.Shuffle());
            if (shuffled != targetPhrase.OriginPhrase)
                break;
        }

        await chat.SendMarkdownMessageAsync(
            QuestionMarkups.FreeTemplateMarkdown(
                chat.Texts.WordsInPhraseAreShuffledWriteThemInOrder.AddEscaped(":")
                    .NewLine() + Markdown.Escaped(shuffled).ToSemiBold()));

        var (result, entry) = await QuestionLogicHelper.GetEnglishUserInputOrIDontKnow(chat,
            QuestionMarkups.FreeTemplateMarkdown(
                chat.Texts.WordsInPhraseAreShuffledWriteThemInOrder.AddEscaped(":")
                    .NewLine() + Markdown.Escaped(shuffled).ToSemiBold()));

        if (result == OptionalUserInputResult.IDontKnow)
            return QuestionResult.Failed(Markdown.Empty, Markdown.Empty);
        if (result == OptionalUserInputResult.NotAnInput) 
            return QuestionResult.RetryThisQuestion;

        var closeness = targetPhrase.OriginPhrase.CheckCloseness(entry.Trim());

        switch (closeness) {
            case StringsCompareResult.Equal:
                return QuestionResult.Passed(chat.Texts);
            case StringsCompareResult.SmallMistakes:
            case StringsCompareResult.BigMistakes:
                await chat.SendMessageAsync(chat.Texts.RetryAlmostRightWithTypo);
                return QuestionResult.RetryThisQuestion;
            case StringsCompareResult.NotEqual:
            default:
                return QuestionResult.Failed(
                    Markdown.Escaped($"{chat.Texts.FailedOriginExampleWas2}:")
                        .NewLine()
                        .AddMarkdown($"\"{targetPhrase.OriginPhrase}\"".ToSemiBoldMarkdown()),
                    chat.Texts);
        }
    }
}