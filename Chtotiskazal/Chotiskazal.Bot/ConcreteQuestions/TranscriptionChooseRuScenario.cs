using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions;

public class TranscriptionChooseRuScenario : IQuestionScenario {
    public QuestionInputType InputType => QuestionInputType.NeedsNoInput;
    public ScenarioWordTypeFit Fit => ScenarioWordTypeFit.OnlyWord;

    public async Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList) {
        var originTranslation = word.RuTranslations.GetRandomItemOrNull();

        if (string.IsNullOrWhiteSpace(originTranslation.Transcription))
            return QuestionResult.Impossible;

        string[] variants = examList
            .Where(e => !e.ContainsTranscription(originTranslation.Transcription))
            .GetRuVariants(originTranslation, 5);

        var msg = QuestionMarkups.TranscriptionTemplate(originTranslation.Transcription,
            chat.Texts.ChooseWhichWordHasThisTranscription);
        await chat.SendMarkdownMessageAsync(msg, InlineButtons.CreateVariants(variants));

        var choice = await chat.TryWaitInlineIntKeyboardInput();
        if (choice == null)
            return QuestionResult.RetryThisQuestion;

        if (word.TextTranslations.Contains(variants[choice.Value]))
            return QuestionResult.Passed(chat.Texts);
        return QuestionResult.Failed(chat.Texts);
    }
}