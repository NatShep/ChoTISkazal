using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll.Services;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions;

public class EngWriteScenario : IQuestionScenario {
    private readonly LocalDictionaryService _localDictionaryService;
    public ScenarioWordTypeFit Fit => ScenarioWordTypeFit.OnlyWord;

    public EngWriteScenario(LocalDictionaryService localDictionaryService) {
        _localDictionaryService = localDictionaryService;
    }

    public QuestionInputType InputType => QuestionInputType.NeedsRuInput;

    public async Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word,
        UserWordModel[] examList) {
        var translations = word.TextTranslations.ToArray();

        var minCount = translations.Min(t => t.Count(c => c == ' '));
        if (minCount > 0 && word.AbsoluteScore < minCount * WordLeaningGlobalSettings.LearningWordMinScore)
            return QuestionResult.Impossible;

        var (result, entry) = await QuestionScenarioHelper.GetRussianUserInputOrIDontKnow(chat,
            QuestionMarkups.TranslateTemplate(word.Word, chat.Texts.WriteTheTranslation));
        
        if(result == OptionalUserInputResult.IDontKnow)
            return Failed();
        if (result == OptionalUserInputResult.NotAnInput)
            return QuestionResult.RetryThisQuestion;

        var (text, comparation) = translations.GetClosestTo(entry.Trim());

        switch (comparation) {
            case StringsCompareResult.Equal:
                return QuestionResult.Passed(chat.Texts);
            case StringsCompareResult.SmallMistakes:
                await chat.SendMarkdownMessageAsync(chat.Texts.YouHaveATypoLetsTryAgain(text));
                return QuestionResult.RetryThisQuestion;
            case StringsCompareResult.BigMistakes:
                return QuestionResult.Failed(Markdown.Escaped(chat.Texts.FailedMistaken(text)),
                    chat.Texts);
        }

        var allMeaningsOfWord = await _localDictionaryService.GetAllTranslationWords(word.Word);
        var (otherMeaning, otherComparation) = allMeaningsOfWord.GetClosestTo(entry);
        if (otherComparation == StringsCompareResult.Equal) {
            await chat.SendMarkdownMessageAsync(
                Markdown.Escaped($"{chat.Texts.OutOfScopeTranslation}: ") +
                Markdown.Escaped(word.AllTranslationsAsSingleString).ToSemiBold());
            return QuestionResult.RetryThisQuestion;
        }

        if (otherComparation == StringsCompareResult.SmallMistakes) {
            await chat.SendMarkdownMessageAsync(
                chat.Texts.OutOfScopeWithCandidate(otherMeaning)
                    .AddEscaped(": ")
                    .AddMarkdown($"\"{word.AllTranslationsAsSingleString}\"".ToSemiBoldMarkdown()));
            return QuestionResult.RetryThisQuestion;
        }

        return Failed();

        QuestionResult Failed() => QuestionResult.Failed(
                Markdown.Escaped(chat.Texts.FailedTranslationWas).NewLine() +
                Markdown.Escaped($"\"{word.AllTranslationsAsSingleString}\"").ToSemiBold(),
                chat.Texts);
    }
}