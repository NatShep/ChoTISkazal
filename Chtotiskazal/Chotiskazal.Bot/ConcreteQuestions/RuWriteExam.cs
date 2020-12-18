using System;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.InterfaceLang;
using SayWhat.Bll;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.Questions
{
    public class RuWriteExam : IExam
    {
        private readonly DictionaryService _dictionaryService;

        public RuWriteExam(DictionaryService dictionaryService)
        {
            _dictionaryService = dictionaryService;
        }
        public bool NeedClearScreen => false;
        public string Name => "Eng Write";

        public async Task<QuestionResult> Pass(ChatIO chatIo, UserWordModel word,
            UserWordModel[] examList)
        {
            var words = word.Word.Split(',').Select(s => s.Trim()).ToArray();
            var minCount = words.Min(t => t.Count(c => c == ' '));
            if (minCount > 0 && word.AbsoluteScore < minCount * WordLeaningGlobalSettings.FamiliarWordMinScore)
                return QuestionResult.Impossible;

            await chatIo.SendMessageAsync($"=====>   {word.TranslationAsList}    <=====\r\n" +
                                          Texts.Current.WriteTheTranslation);
            var userEntry = await chatIo.WaitUserTextInputAsync();

            if (string.IsNullOrEmpty(userEntry))
                return QuestionResult.Retry;

            var (text, comparation) = words.GetClosestTo(userEntry.Trim());
            
            if (comparation == StringsCompareResult.Equal)
                return QuestionResult.Passed;

            if (comparation == StringsCompareResult.SmallMistakes)
            {
                await chatIo.SendMessageAsync(Texts.Current.YouHaveATypoLetsTryAgain(text));
                return QuestionResult.Retry;
            }
            
            if (comparation == StringsCompareResult.BigMistakes)
                return QuestionResult.FailedText(Texts.Current.FailedMistaken(text), Texts.Current.Mistaken);

            //search for other translation
            var translationCandidate = await _dictionaryService.GetAllTranslationWords(userEntry.ToLower());
            
            if (translationCandidate.Any(t1 =>
                word.AllTranslations.Any(t2 => t1.Trim().AreEqualIgnoreCase(t2.Trim()))))
            {
                //translation is correct, but for other word
                await chatIo.SendMessageAsync(
                    
                    $"{Texts.Current.CorrectTranslationButQuestionWasAbout} '{word.Word} - {word.TranslationAsList}'\r\n" +
                       Texts.Current.LetsTryAgain);
                return QuestionResult.Retry;
            }
            
            var translates = string.Join(",",translationCandidate);
            string failedMessage = "";
            if(!string.IsNullOrWhiteSpace(translates))
                failedMessage =$"{userEntry} {Texts.Current.translatesAs} {translates}\r\n";
            failedMessage += $"{Texts.Current.RightTranslationWas}: {word.Word}";
            return QuestionResult.FailedText(failedMessage);
        }
    }
}