using System;
using System.Linq;
using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.Questions
{
    public class EngWriteExam : IExam
    {
        private readonly DictionaryService _dictionaryService;

        public EngWriteExam(DictionaryService dictionaryService)
        {
            _dictionaryService = dictionaryService;
        }
        public bool NeedClearScreen => false;

        public string Name => "Eng Write";

        public async Task<ExamResult> Pass(ChatIO chatIo, UserWordModel word,
            UserWordModel[] examList)
        {
            var translations = word.AllTranslations.ToArray();
            
            var minCount = translations.Min(t => t.Count(c => c == ' '));
            if (minCount > 0 && word.AbsoluteScore < minCount * WordLeaningGlobalSettings.FamiliarWordMinScore)
                return ExamResult.Impossible;

            await chatIo.SendMessageAsync($"=====>   {word.Word}    <=====\r\n" +
                                          $"Write the translation... ");
            var translation = await chatIo.WaitUserTextInputAsync();
           
            if (string.IsNullOrEmpty(translation))
                return ExamResult.Retry;

            var (text,comparation) =  translations.GetClosestTo(translation.Trim());
            
            if (comparation== StringsCompareResult.Equal)
                return ExamResult.Passed;
            if (comparation == StringsCompareResult.SmallMistakes)
            {
                await chatIo.SendMessageAsync($"You have a typo. Correct spelling is '{text}'.");
                return ExamResult.Impossible;
            }
            if (comparation == StringsCompareResult.BigMistakes)
            {
                await chatIo.SendMessageAsync($"Mistaken. Correct spelling is '{text}'");
                return ExamResult.Failed;
            }

            
            var allMeaningsOfWord = await _dictionaryService.GetAllTranslationWords(word.Word);

            var (otherMeaning, otherComparation) = allMeaningsOfWord.GetClosestTo(translation);
            
            if (otherComparation == StringsCompareResult.Equal)
            {
                await chatIo.SendMessageAsync(
                    $"Chosen translation is out of scope (but it is correct). Expected translations are: " +
                    word.TranslationAsList);
                return ExamResult.Impossible;
            }
            if (otherComparation == StringsCompareResult.SmallMistakes)
            {
                await chatIo.SendMessageAsync(
                    $"Chosen translation is out of scope (did you mean '{otherMeaning}'?). Expected translations are: " +
                    word.TranslationAsList);
                return ExamResult.Impossible;
            }

            await chatIo.SendMessageAsync("The translation was: " + word.TranslationAsList);
            return ExamResult.Failed;
        }
    }
}