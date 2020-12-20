using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.InterfaceLang;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions
{
    public class EngWriteQuestion : IQuestion
    {
        private readonly DictionaryService _dictionaryService;

        public EngWriteQuestion(DictionaryService dictionaryService)
        {
            _dictionaryService = dictionaryService;
        }
        public bool NeedClearScreen => false;

        public string Name => "Eng Write";

        public async Task<QuestionResult> Pass(ChatIO chatIo, UserWordModel word,
            UserWordModel[] examList)
        {
            var translations = word.TextTranslations.ToArray();
            
            var minCount = translations.Min(t => t.Count(c => c == ' '));
            if (minCount > 0 && word.AbsoluteScore < minCount * WordLeaningGlobalSettings.FamiliarWordMinScore)
                return QuestionResult.Impossible;

            await chatIo.SendMessageAsync($"=====>   {word.Word}    <=====\r\n{Texts.Current.WriteTheTranslation}");
            var translation = await chatIo.WaitUserTextInputAsync();
           
            if (string.IsNullOrEmpty(translation))
                return QuestionResult.RetryThisQuestion;

            var (text,comparation) =  translations.GetClosestTo(translation.Trim());
            
            switch (comparation)
            {
                case StringsCompareResult.Equal:
                    return QuestionResult.Passed;
                case StringsCompareResult.SmallMistakes:
                    await chatIo.SendMessageAsync(Texts.Current.YouHaveATypoLetsTryAgain(text));
                    return QuestionResult.RetryThisQuestion;
                case StringsCompareResult.BigMistakes:
                    return QuestionResult.FailedText(Texts.Current.FailedMistaken(text));
            }
            var allMeaningsOfWord = await _dictionaryService.GetAllTranslationWords(word.Word);
            var (otherMeaning, otherComparation) = allMeaningsOfWord.GetClosestTo(translation);
            if (otherComparation == StringsCompareResult.Equal) 
            {
                await chatIo.SendMessageAsync(
                    $"{Texts.Current.OutOfScopeTranslation}: " +
                    word.AllTranslationsAsSingleString);
                return QuestionResult.RetryThisQuestion;
            }
            if (otherComparation == StringsCompareResult.SmallMistakes)
            {
                await chatIo.SendMessageAsync(
                    Texts.Current.OutOfScopeWithCandidate(otherMeaning)+": "+
                    word.AllTranslationsAsSingleString);
                return QuestionResult.RetryThisQuestion;
            }

            return QuestionResult.FailedText(Texts.Current.FailedTranslationWas +$" '{word.AllTranslationsAsSingleString}'");
        }
    }
}