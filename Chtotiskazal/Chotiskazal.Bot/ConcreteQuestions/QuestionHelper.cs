using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions {
public static class QuestionHelper {
    public static async Task<QuestionResult> TryReplyWriteQuestionIfResultIsCorrectOrSemiCorrect(ChatRoom chat, string correctSpelling, StringsCompareResult inputCloseness) {
        switch (inputCloseness) {
            case StringsCompareResult.Equal:
                return QuestionResult.Passed(chat.Texts);
            case StringsCompareResult.SmallMistakes:
                await chat.SendMarkdownMessageAsync(chat.Texts.YouHaveATypoLetsTryAgainMarkdown(correctSpelling));
                return QuestionResult.RetryThisQuestion;
            case StringsCompareResult.BigMistakes:
                return QuestionResult.Failed(chat.Texts.FailedMistakenMarkdown(correctSpelling),
                    chat.Texts);
            default:
                return null;
        }
    }
}
}