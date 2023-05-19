using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Interface;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll;
using SayWhat.Bll.Services;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions {

public class RuWriteQuestion : IQuestion {
    private readonly LocalDictionaryService _localDictionaryService;

    public RuWriteQuestion(LocalDictionaryService localDictionaryService) {
        _localDictionaryService = localDictionaryService;
    }

    public bool NeedClearScreen => false;
    public string Name => "Ru Write";

    public Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList) =>
        RuWriteQuestionHelper.PassRuWriteQuestion(
            chat,
            word,
            word.AllTranslationsAsSingleString,
            _localDictionaryService);
}

public class RuWriteSingleTarnslationQuestion : IQuestion {
    private readonly LocalDictionaryService _localDictionaryService;

    public RuWriteSingleTarnslationQuestion(LocalDictionaryService localDictionaryService) {
        _localDictionaryService = localDictionaryService;
    }

    public bool NeedClearScreen => false;
    public string Name => "Ru Write Single Translation";

    public Task<QuestionResult> Pass(
        ChatRoom chat, UserWordModel word,
        UserWordModel[] examList) =>
        RuWriteQuestionHelper.PassRuWriteQuestion(
            chat, word, word.RuTranslations.GetRandomItemOrNull().Word, _localDictionaryService);
}

static class RuWriteQuestionHelper {
    public static async Task<QuestionResult> PassRuWriteQuestion(
        ChatRoom chat,
        UserWordModel word,
        string ruTranslationCaption,
        LocalDictionaryService localDictionaryService) {
        var wordsInPhraseCount = word.Word.Count(c => c == ' ');
        if (wordsInPhraseCount > 0 &&
            word.AbsoluteScore < wordsInPhraseCount * WordLeaningGlobalSettings.LearningWordMinScore)
            return QuestionResult.Impossible;

        await chat.SendMarkdownMessageAsync(
            QuestionMarkups.TranslateTemplate(ruTranslationCaption, chat.Texts.WriteTheTranslation));

        var enUserEntry = await chat.WaitUserTextInputAsync();

        if (string.IsNullOrEmpty(enUserEntry))
            return QuestionResult.RetryThisQuestion;
        
        if (enUserEntry.IsRussian())
        {
            await chat.SendMessageAsync(chat.Texts.EnglishInputExpected);
            return QuestionResult.RetryThisQuestion;
        }
        
        var comparation = word.Word.CheckCloseness(enUserEntry);

        if (comparation == StringsCompareResult.Equal)
            return QuestionResult.Passed(chat.Texts);

        if (comparation == StringsCompareResult.SmallMistakes)
        {
            await chat.SendMarkdownMessageAsync(chat.Texts.YouHaveATypoLetsTryAgain(word.Word));
            return QuestionResult.RetryThisQuestion;
        }

        if (comparation == StringsCompareResult.BigMistakes)
            return QuestionResult.Failed(Markdown.Escaped(chat.Texts.FailedMistaken(word.Word)), Markdown.Escaped(chat.Texts.Mistaken));
        
        // ## Other translation case ##

        // example: 
        //     Coefficient - коэффициент
        //     Rate        - коэффициент
        // Question is about 'коэффициент' (Coefficient)
        // User answers 'Rate'
        // Search for 'Rate' translations
        var otherRuTranslationsOfUserInput = await localDictionaryService.GetAllTranslationWords(enUserEntry.ToLower());
        
        // if otherRuTranslationsOfUserInput contains 'коэффициент' or something like it
        // then retry question
        var russianTranslations = word.TextTranslations;
        if (russianTranslations.Any(
                t1 =>
                    otherRuTranslationsOfUserInput.Any(t1.AreEqualIgnoreSmallMistakes)))
        {
            //translation is correct, but for other word
            await chat.SendMessageAsync(
                $"{chat.Texts.CorrectTranslationButQuestionWasAbout} \"{word.Word}\" - *{word.AllTranslationsAsSingleString}*'\r\n" +
                chat.Texts.LetsTryAgain);
            return QuestionResult.RetryThisQuestion;
        }

        var translates = string.Join(",", otherRuTranslationsOfUserInput);
       
        Markdown failedMessage = Markdown.Empty;
        if (!string.IsNullOrWhiteSpace(translates))
            failedMessage = Markdown.Escaped($"{enUserEntry} {chat.Texts.translatesAs} {translates}").NewLine();
        failedMessage += Markdown.Escaped($"{chat.Texts.RightTranslationWas}: ") +
                         Markdown.Escaped($"\"{word.Word}\"").ToSemiBold();
        
        return QuestionResult.Failed(
            failedMessage,
            chat.Texts);
    }
}

}