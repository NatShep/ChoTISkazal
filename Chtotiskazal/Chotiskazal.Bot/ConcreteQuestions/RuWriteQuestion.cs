using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Interface;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll;
using SayWhat.Bll.Services;
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

    public Task<QuestionResultMarkdown> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList) =>
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

    public Task<QuestionResultMarkdown> Pass(
        ChatRoom chat, UserWordModel word,
        UserWordModel[] examList) =>
        RuWriteQuestionHelper.PassRuWriteQuestion(
            chat, word, word.RuTranslations.GetRandomItemOrNull().Word, _localDictionaryService);
}

static class RuWriteQuestionHelper {
    public static async Task<QuestionResultMarkdown> PassRuWriteQuestion(
        ChatRoom chat,
        UserWordModel word,
        string ruTranslationCaption,
        LocalDictionaryService localDictionaryService) {
        var wordsInPhraseCount = word.Word.Count(c => c == ' ');
        if (wordsInPhraseCount > 0 &&
            word.AbsoluteScore < wordsInPhraseCount * WordLeaningGlobalSettings.FamiliarWordMinScore)
            return QuestionResultMarkdown.Impossible;

        await chat.SendMarkdownMessageAsync(
            QuestionMarkups.TranslateTemplate(ruTranslationCaption, chat.Texts.WriteTheTranslation));

        var enUserEntry = await chat.WaitUserTextInputAsync();

        if (string.IsNullOrEmpty(enUserEntry))
            return QuestionResultMarkdown.RetryThisQuestion;
        
        if (enUserEntry.IsRussian())
        {
            await chat.SendMessageAsync(chat.Texts.EnglishInputExpected);
            return QuestionResultMarkdown.RetryThisQuestion;
        }
        
        var comparation = word.Word.CheckCloseness(enUserEntry);

        if (comparation == StringsCompareResult.Equal)
            return QuestionResultMarkdown.Passed(chat.Texts);

        if (comparation == StringsCompareResult.SmallMistakes)
        {
            await chat.SendMarkdownMessageAsync(chat.Texts.YouHaveATypoLetsTryAgainMarkdown(word.Word));
            return QuestionResultMarkdown.RetryThisQuestion;
        }

        if (comparation == StringsCompareResult.BigMistakes)
            return QuestionResultMarkdown.Failed(MarkdownObject.Escaped(chat.Texts.FailedMistaken(word.Word)), MarkdownObject.Escaped(chat.Texts.Mistaken));
        
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
            return QuestionResultMarkdown.RetryThisQuestion;
        }

        var translates = string.Join(",", otherRuTranslationsOfUserInput);
       
        MarkdownObject failedMessage = MarkdownObject.Empty();
        if (!string.IsNullOrWhiteSpace(translates))
            failedMessage = MarkdownObject.Escaped($"{enUserEntry} {chat.Texts.translatesAs} {translates}").AddNewLine();
        failedMessage += MarkdownObject.Escaped($"{chat.Texts.RightTranslationWas}: ") +
                         MarkdownObject.Escaped($"\"{word.Word}\"").ToSemiBold();
        
        return QuestionResultMarkdown.Failed(
            failedMessage,
            chat.Texts);
    }
}

}