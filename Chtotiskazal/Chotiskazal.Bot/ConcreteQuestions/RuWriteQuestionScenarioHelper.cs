using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll.Services;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions;

static class RuWriteQuestionScenarioHelper {
    public static async Task<QuestionResult> PassRuWriteQuestion(
        ChatRoom chat,
        UserWordModel word,
        string ruTranslationCaption,
        LocalDictionaryService localDictionaryService) {
        var wordsInPhraseCount = word.Word.Count(c => c == ' ');
        if (wordsInPhraseCount > 0 &&
            word.AbsoluteScore < wordsInPhraseCount * WordLeaningGlobalSettings.LearningWordMinScore)
            return QuestionResult.Impossible;

        var (result, input) = await QuestionScenarioHelper.GetEnglishUserInputOrIDontKnow(chat,
            QuestionMarkups.TranslateTemplate(ruTranslationCaption, chat.Texts.WriteTheTranslation));
        if(result== OptionalUserInputResult.IDontKnow)
            return QuestionResult.Failed(Markdown.Empty, Markdown.Empty);
        if(result == OptionalUserInputResult.NotAnInput)
            return QuestionResult.RetryThisQuestion;

        var comparation = word.Word.CheckCloseness(input);

        if (comparation == StringsCompareResult.Equal)
            return QuestionResult.Passed(chat.Texts);

        if (comparation == StringsCompareResult.SmallMistakes) {
            await chat.SendMarkdownMessageAsync(chat.Texts.YouHaveATypoLetsTryAgain(word.Word));
            return QuestionResult.RetryThisQuestion;
        }

        if (comparation == StringsCompareResult.BigMistakes)
            return QuestionResult.Failed(Markdown.Escaped(chat.Texts.FailedMistaken(word.Word)),
                Markdown.Escaped(chat.Texts.Mistaken));

        // ## Other translation case ##

        // example: 
        //     Coefficient - коэффициент
        //     Rate        - коэффициент
        // Question is about 'коэффициент' (Coefficient)
        // User answers 'Rate'
        // Search for 'Rate' translations
        var otherRuTranslationsOfUserInput = await localDictionaryService.GetAllTranslationWords(input.ToLower());

        // if otherRuTranslationsOfUserInput contains 'коэффициент' or something like it
        // then retry question
        var russianTranslations = word.TextTranslations;
        if (russianTranslations.Any(
                t1 =>
                    otherRuTranslationsOfUserInput.Any(t1.AreEqualIgnoreSmallMistakes))) {
            //translation is correct, but for other word
            await chat.SendMessageAsync(
                $"{chat.Texts.CorrectTranslationButQuestionWasAbout} \"{word.Word}\" - *{word.AllTranslationsAsSingleString}*'\r\n" +
                chat.Texts.LetsTryAgain);
            return QuestionResult.RetryThisQuestion;
        }

        var translates = string.Join(",", otherRuTranslationsOfUserInput);

        Markdown failedMessage = Markdown.Empty;
        if (!string.IsNullOrWhiteSpace(translates))
            failedMessage = Markdown.Escaped($"{input} {chat.Texts.translatesAs} {translates}").NewLine();
        failedMessage += Markdown.Escaped($"{chat.Texts.RightTranslationWas}: ") +
                         Markdown.Escaped($"\"{word.Word}\"").ToSemiBold();

        return QuestionResult.Failed(
            failedMessage,
            chat.Texts);
    }
}