using System;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Services;
using SayWhat.Bll;
using SayWhat.Bll.Dto;

namespace Chotiskazal.Bot.Questions
{
    public class AssemblePhraseExam :IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Assemble phrase";

        public async Task<ExamResult> Pass(ChatIO chatIo, ExamService service, UserWordModel word, UserWordModel[] examList) 
        {
            if (!word.HasAnyPhrases)
                return ExamResult.Impossible;

            var targetPhrase = word.GetRandomExample();

            string shuffled;
            while (true)
            {
                var wordsInExample = targetPhrase.OriginWords;
                
                if (wordsInExample.Length < 2)
                    return ExamResult.Impossible;

                shuffled = string.Join(" ", wordsInExample.Randomize());
                if(shuffled!= targetPhrase.OriginPhrase)
                    break;
            }

            await chatIo.SendMessageAsync("Words in phrase are shuffled. Write them in correct order:\r\n'" +  shuffled+ "'");
            string entry= null;
            while (string.IsNullOrWhiteSpace(entry))
            {
                entry = await chatIo.WaitUserTextInputAsync();
                entry = entry.Trim();
            }

            if (string.CompareOrdinal(targetPhrase.OriginPhrase, entry) == 0)
            {
                await service.RegisterSuccessAsync(word);
                return ExamResult.Passed;
            }

            await chatIo.SendMessageAsync($"Original phrase was: '{targetPhrase.OriginPhrase}'");
            await service.RegisterFailureAsync(word);
            return ExamResult.Failed;
        }
    }
}