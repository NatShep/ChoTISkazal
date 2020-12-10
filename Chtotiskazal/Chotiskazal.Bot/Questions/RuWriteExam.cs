using System;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task<ExamResult> Pass(ChatIO chatIo, UserWordModel word,
            UserWordModel[] examList)
        {
            var words = word.Word.Split(',').Select(s => s.Trim()).ToArray();
            var minCount = words.Min(t => t.Count(c => c == ' '));
            if (minCount > 0 && word.AbsoluteScore < minCount * 4)
                return ExamResult.Impossible;

            await chatIo.SendMessageAsync($"=====>   {word.TranslationAsList}    <=====\r\n" +
                                          $"Write the translation... ");
            var userEntry = await chatIo.WaitUserTextInputAsync();

            if (string.IsNullOrEmpty(userEntry))
                return ExamResult.Retry;

            if (words.Any(t => string.Compare(userEntry.Trim(), t, StringComparison.OrdinalIgnoreCase) == 0))
            {
                return ExamResult.Passed;
            }
            //search for other translation
            var translationCandidate = await _dictionaryService.GetAllTranslationWords(userEntry.ToLower());
            
            if (translationCandidate.Any(t1 =>
                word.AllTranslations.Any(t2 => string.CompareOrdinal(t1.Trim(), t2.Trim()) == 0)))
            {
                //translation is correct, but for other word
                await chatIo.SendMessageAsync(
                    $"the translation was correct, but the question was about the word '{word.Word} - {word.TranslationAsList}'\r\nlet's try again");
                return ExamResult.Retry;
            }
            
            var translates = string.Join(",",translationCandidate);
            if(!string.IsNullOrWhiteSpace(translates))
                await chatIo.SendMessageAsync($"'{userEntry}' translates as {translates}");
            await chatIo.SendMessageAsync("The right translation was: " + word.Word);
            return ExamResult.Failed;
        }
    }
}