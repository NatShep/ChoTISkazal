using System;
using System.Linq;
using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.Bll.Dto;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.Questions
{
    public class EngChoosePhraseExam : IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Eng Choose Phrase";

        public async Task<ExamResult> Pass(
            ChatIO chatIo, 
            UsersWordsService service, 
            UserWordModel word, 
            UserWordModel[] examList)
        {
            if (!word.HasAnyExamples)
                return ExamResult.Impossible;
            
            var targetPhrase = word.GetRandomExample();

            var otherExamples = examList
                .SelectMany(e => e.Examples)
                .Where(p => !string.Equals(p.TranslatedPhrase, targetPhrase.TranslatedPhrase,StringComparison.InvariantCultureIgnoreCase))
                .Take(8).ToArray();

            if(!otherExamples.Any())
                return ExamResult.Impossible;
            
            var variants = otherExamples
                .Append(targetPhrase)
                .Randomize()
                .Select(e => e.TranslatedPhrase)
                .ToArray();
            
            var msg = $"=====>   {targetPhrase.OriginPhrase}    <=====\r\n" +
                      $"Choose the translation";
            await chatIo.SendMessageAsync(msg, InlineButtons.CreateVariants(variants));
            
            var choice = await chatIo.TryWaitInlineIntKeyboardInput();
            if (choice == null)
                return ExamResult.Retry;
            
            return string.Equals(
                    variants[choice.Value], 
                    targetPhrase.TranslatedPhrase, 
                    StringComparison.InvariantCultureIgnoreCase) 
                ? ExamResult.Passed 
                : ExamResult.Failed;
        }
    }
}