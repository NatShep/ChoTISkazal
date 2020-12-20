using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.InterfaceLang;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions
{
    public class RuWriteQuestion : IQuestion
    {
        private readonly DictionaryService _dictionaryService;

        public RuWriteQuestion(DictionaryService dictionaryService)
        {
            _dictionaryService = dictionaryService;
        }
        public bool NeedClearScreen => false;
        public string Name => "Eng Write";

        public async Task<QuestionResult> Pass(ChatIO chatIo, UserWordModel word,
            UserWordModel[] examList)
        {
            var wordsInPhraseCount = word.Word.Count(c => c == ' ');
            if (wordsInPhraseCount > 0 
                && word.AbsoluteScore < wordsInPhraseCount * WordLeaningGlobalSettings.FamiliarWordMinScore)
                return QuestionResult.Impossible;

            await chatIo.SendMessageAsync($"=====>   {word.AllTranslationsAsSingleString}    <=====\r\n" +
                                          Texts.Current.WriteTheTranslation);
            var enUserEntry = await chatIo.WaitUserTextInputAsync();

            if (string.IsNullOrEmpty(enUserEntry))
                return QuestionResult.RetryThisQuestion;

            var comparation = word.Word.CheckForMistakes(enUserEntry);
            
            if (comparation == StringsCompareResult.Equal)
                return QuestionResult.Passed;

            if (comparation == StringsCompareResult.SmallMistakes)
            {
                await chatIo.SendMessageAsync(Texts.Current.YouHaveATypoLetsTryAgain(word.Word));
                return QuestionResult.RetryThisQuestion;
            }
            
            if (comparation == StringsCompareResult.BigMistakes)
                return QuestionResult.FailedText(Texts.Current.FailedMistaken(word.Word), Texts.Current.Mistaken);
            
            
            // ## Other translation case ##
            
            // example: 
            //     Coefficient - коэффициент
            //     Rate        - коэффициент
            // Question is about 'коэффициент' (Coefficient)
            // User answers 'Rate'
            // Search for 'Rate' translations
            var otherRuTranslationsOfUserInput = await _dictionaryService.GetAllTranslationWords(enUserEntry.ToLower());
            // if otherRuTranslationsOfUserInput contains 'коэффициент' or something like it
            // then retry question
            if (word.TextTranslations.Any(t1 =>
                otherRuTranslationsOfUserInput.Any(t1.AreEqualIgnoreSmallMistakes)))
            {
                //translation is correct, but for other word
                await chatIo.SendMessageAsync(
                    $"{Texts.Current.CorrectTranslationButQuestionWasAbout} '{word.Word} - {word.AllTranslationsAsSingleString}'\r\n" +
                       Texts.Current.LetsTryAgain);
                return QuestionResult.RetryThisQuestion;
            }
            
            var translates = string.Join(",",otherRuTranslationsOfUserInput);
            string failedMessage = "";
            if(!string.IsNullOrWhiteSpace(translates))
                failedMessage =$"{enUserEntry} {Texts.Current.translatesAs} {translates}\r\n";
            failedMessage += $"{Texts.Current.RightTranslationWas}: {word.Word}";
            return QuestionResult.FailedText(failedMessage);
        }
    }
}