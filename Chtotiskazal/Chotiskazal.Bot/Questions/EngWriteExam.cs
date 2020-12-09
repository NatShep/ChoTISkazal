using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Serializers;
using SayWhat.Bll;
using SayWhat.Bll.Dto;
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

        public async Task<ExamResult> Pass(ChatIO chatIo, UsersWordsService service, UserWordModel word,
            UserWordModel[] examList)
        {
            var translations = word.AllTranslations.ToArray();
            
            var minCount = translations.Min(t => t.Count(c => c == ' '));
            if (minCount > 0 && word.AbsoluteScore < minCount * 4)
                return ExamResult.Impossible;

            await chatIo.SendMessageAsync($"=====>   {word.Word}    <=====\r\n" +
                                          $"Write the translation... ");
            var translation = await chatIo.WaitUserTextInputAsync();
           
            if (string.IsNullOrEmpty(translation))
                return ExamResult.Retry;

            if (translations.Any(t => string.Compare(translation.Trim(), t, StringComparison.OrdinalIgnoreCase) == 0))
            {
                return ExamResult.Passed;
            }

            var allMeaningsOfWord = await _dictionaryService.GetAllTranslationWords(word.Word);
          
            if (allMeaningsOfWord
                .Any(t => string.Compare(translation, t, StringComparison.OrdinalIgnoreCase) == 0))
            {
                await chatIo.SendMessageAsync(
                    $"Chosen translation is out of scope (but it is correct). Expected translations are: " +
                    word.TranslationAsList);
                return ExamResult.Impossible;
            }

            await chatIo.SendMessageAsync("The translation was: " + word.TranslationAsList);
            return ExamResult.Failed;
        }
    }
}