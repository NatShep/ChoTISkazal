using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions;

public class TranscriptionChooseEngLogic : IQuestionLogic {
    public async Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList) {
        var originTranslation = word.RuTranslations.Where(r => r.HasTranscription).GetRandomItemOrNull();

        if (originTranslation == null)
            return QuestionResult.Impossible;

        string[] variants = examList
            .Where(e => !e.ContainsTranscription(originTranslation.Transcription))
            .GetEngVariants(word.Word, 5);

        var msg = QuestionMarkups.TranscriptionTemplate(originTranslation.Transcription,
            chat.Texts.ChooseWhichWordHasThisTranscription);
        await chat.SendMarkdownMessageAsync(msg, InlineButtons.CreateVariants(variants));

        var choice = await chat.TryWaitInlineIntKeyboardInput();
        if (choice == null)
            return QuestionResult.RetryThisQuestion;
        if (word.Word.Equals(variants[choice.Value]))
            return QuestionResult.Passed(chat.Texts);
        return QuestionResult.Failed(chat.Texts);
    }
}