using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll.Services;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions;

public class EngWriteLogic : IQuestionLogic {
    private readonly LocalDictionaryService _localDictionaryService;

    public EngWriteLogic(LocalDictionaryService localDictionaryService) {
        _localDictionaryService = localDictionaryService;
    }

    public async Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word,
        UserWordModel[] examList) {
        var translations = word.TextTranslations.ToArray();

        var minCount = translations.Min(t => t.Count(c => c == ' '));
        if (minCount > 0 && word.AbsoluteScore < minCount * WordLeaningGlobalSettings.LearningWordMinScore)
            return QuestionResult.Impossible;


        await chat.SendMarkdownMessageAsync(
            QuestionMarkups.TranslateTemplate(word.Word, chat.Texts.WriteTheTranslation));
        var entry = await chat.WaitUserTextInputAsync();

        if (string.IsNullOrEmpty(entry))
            return QuestionResult.RetryThisQuestion;

        if (!entry.IsRussian()) {
            await chat.SendMessageAsync(chat.Texts.RussianInputExpected);
            return QuestionResult.RetryThisQuestion;
        }

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

        return QuestionResult.Failed(
            Markdown.Escaped(chat.Texts.FailedTranslationWas).NewLine() +
            Markdown.Escaped($"\"{word.AllTranslationsAsSingleString}\"").ToSemiBold(),
            chat.Texts);
    }
}