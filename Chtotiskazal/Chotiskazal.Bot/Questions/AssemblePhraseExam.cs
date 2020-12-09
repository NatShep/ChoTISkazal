using System;
using System.Linq;
using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.Bll.Dto;
using SayWhat.Bll.Services;

namespace Chotiskazal.Bot.Questions
{
    public class AssemblePhraseExam :IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Assemble phrase";

        public async Task<ExamResult> Pass(ChatIO chatIo, UsersWordsService service, UserWordModel word, UserWordModel[] examList) 
        {
            if (!word.HasAnyPhrases)
                return ExamResult.Impossible;

            var targetPhrase = word.GetRandomExample();

            string shuffled;
            while (true)
            {
                var wordsInExample = targetPhrase.SplitWordsOfPhrase;
                
                if (wordsInExample.Length < 2)
                    return ExamResult.Impossible;

                shuffled = string.Join(" ", wordsInExample.Randomize());
                if(shuffled!= targetPhrase.OriginPhrase)
                    break;
            }

            await chatIo.SendMessageAsync("Words in phrase are shuffled. Write them in correct order:\r\n'" +  shuffled+ "'");
            var entry = await chatIo.WaitUserTextInputAsync();
            entry = entry.Trim();

            if (string.Compare(targetPhrase.OriginPhrase, entry.Trim(), StringComparison.OrdinalIgnoreCase) == 0)
            {
                return ExamResult.Passed;
            }

            await chatIo.SendMessageAsync($"Original phrase was: '{targetPhrase.OriginPhrase}'");
            return ExamResult.Failed;
        }
    }
}