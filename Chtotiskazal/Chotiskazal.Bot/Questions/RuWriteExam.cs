using System;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Services;
using Chotiskazal.Dal.DAL;

namespace Chotiskazal.Bot.Questions
{
    public class RuWriteExam : IExam
    {
        public bool NeedClearScreen => false;
        public string Name => "Eng Write";

        public async Task<ExamResult> Pass(ChatIO chatIo, ExamService service, UserWordForLearning word,
            UserWordForLearning[] examList)
        {
            var words = word.EnWord.Split(',').Select(s => s.Trim()).ToArray();
            var minCount = words.Min(t => t.Count(c => c == ' '));
            if (minCount > 0 && word.PassedScore < minCount * 4)
                return ExamResult.Impossible;

            await chatIo.SendMessageAsync($"=====>   {word.UserTranslations}    <=====\r\nWrite the translation... ");
            var userEntry = await chatIo.WaitUserTextInputAsync();

            if (string.IsNullOrEmpty(userEntry))
                return ExamResult.Retry;

            if (words.Any(t => string.Compare(userEntry, t, StringComparison.OrdinalIgnoreCase) == 0))
            {
                await service.RegisterSuccessAsync(word);
                return ExamResult.Passed;
            }
            //search for other translation
            var translationCandidate = await service.GetAllMeaningOfWordForExamination(userEntry.ToLower());

            if (translationCandidate.Any(t1 =>
                word.GetTranslations().Any(t2 => string.CompareOrdinal(t1.Trim(), t2.Trim()) == 0)))
            {
                //translation is correct, but for other word
                await chatIo.SendMessageAsync(
                    $"the translation was correct, but the question was about the word '{word.EnWord} - {word.UserTranslations}'\r\nlet's try again");
                return ExamResult.Retry;
            }
            
            var translates = string.Join(",",translationCandidate);
            await chatIo.SendMessageAsync($"'{userEntry}' translates as {translates}");
            await chatIo.SendMessageAsync("The right translation was: " + word.EnWord);
            await service.RegisterFailureAsync(word);
            return ExamResult.Failed;
        }
    }
}