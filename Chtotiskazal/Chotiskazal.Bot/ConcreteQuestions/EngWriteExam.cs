using System;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.InterfaceLang;
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

        public async Task<QuestionResult> Pass(ChatIO chatIo, UserWordModel word,
            UserWordModel[] examList)
        {
            var translations = word.AllTranslations.ToArray();
            
            var minCount = translations.Min(t => t.Count(c => c == ' '));
            if (minCount > 0 && word.AbsoluteScore < minCount * WordLeaningGlobalSettings.FamiliarWordMinScore)
                return QuestionResult.Impossible;

            await chatIo.SendMessageAsync($"=====>   {word.Word}    <=====\r\n" +
                                          $"Write the translation... ");
            var translation = await chatIo.WaitUserTextInputAsync();
           
            if (string.IsNullOrEmpty(translation))
                return QuestionResult.Retry;

            var (text,comparation) =  translations.GetClosestTo(translation.Trim());
            
            if (comparation== StringsCompareResult.Equal)
                return QuestionResult.Passed;
            if (comparation == StringsCompareResult.SmallMistakes)
            {
                await chatIo.SendMessageAsync($"You have a typo. Correct spelling is '{text}'.");
                return QuestionResult.Impossible;
            }
            if (comparation == StringsCompareResult.BigMistakes)
            {
                await chatIo.SendMessageAsync($"Mistaken. Correct spelling is '{text}'");
                return QuestionResult.Failed;
            }

            
            var allMeaningsOfWord = await _dictionaryService.GetAllTranslationWords(word.Word);

            var (otherMeaning, otherComparation) = allMeaningsOfWord.GetClosestTo(translation);
            
            if (otherComparation == StringsCompareResult.Equal)
            {
                await chatIo.SendMessageAsync(
                    $"{Texts.Current.OutOfScopeTranslation}: " +
                    word.TranslationAsList);
                return QuestionResult.Impossible;
            }
            if (otherComparation == StringsCompareResult.SmallMistakes)
            {
                await chatIo.SendMessageAsync(
                    Texts.Current.OutOfScopeWithCandidate(otherMeaning)+": "+
                    word.TranslationAsList);
                return QuestionResult.Impossible;
            }

            return QuestionResult.FailedText(Texts.Current.FailedTranslationWas +$" '{word.TranslationAsList}'");
        }
    }
}