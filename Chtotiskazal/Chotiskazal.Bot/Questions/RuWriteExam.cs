using System;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.ConsoleTesting.Services;
using Chotiskazal.DAL;

namespace Chotiskazal.Bot.Questions
{
    public class RuWriteExam : IExam
    {
        public bool NeedClearScreen => false;
        public string Name => "Eng Write";

        public async Task<ExamResult> Pass(ChatIO chatIo, ExamService service, UserWordForLearning word, UserWordForLearning[] examList)
        {
            var words = word.EnWord.Split(',').Select(s => s.Trim());
            var minCount = words.Min(t => t.Count(c => c == ' '));
            if (minCount > 0 && word.PassedScore < minCount * 4)
                return ExamResult.Impossible;

            await chatIo.SendMessageAsync($"=====>   {word.UserTranslations}    <=====\r\nWrite the translation... ");
            var userEntry = await chatIo.WaitUserTextInputAsync();
            if (string.IsNullOrEmpty(userEntry))
                return ExamResult.Retry;

            if (words.Any(t => string.Compare(userEntry, t, StringComparison.OrdinalIgnoreCase) == 0))
            {
                await service.RegistrateSuccessAsync(word);
                return ExamResult.Passed;
            }
            else
            {
                //search for other translation
                //TODO 
                /*
                var translationCandidate = service.Get(userEntry.ToLower());
                if (translationCandidate != null)
                {

                    if (translationCandidate.GetTranslations().Any(t1=> word.GetTranslations().Any(t2=> string.CompareOrdinal(t1.Trim(), t2.Trim())==0)))
                    {
                        //translation is correct, but for other word
                        await chat.SendMessage($"the translation was correct, but the question was about the word '{word.OriginWord}'\r\nlet's try again");
                        //Console.ReadLine();
                        return ExamResult.Retry;
                    }
                    else
                    {
                        await chat.SendMessage($"'{userEntry}' translates as {translationCandidate.Translation}");
                        service.RegistrateFailure(word);
                        return ExamResult.Failed;
                    }
                }
                */
                
                await chatIo.SendMessageAsync("The translation was: " + word.EnWord);
                await service.RegistrateFailureAsync(word);
                return ExamResult.Failed;
            }
        }
    }
}