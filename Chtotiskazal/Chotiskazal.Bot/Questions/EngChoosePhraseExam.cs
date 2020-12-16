using System;
using System.Linq;
using System.Threading.Tasks;
using SayWhat.Bll;
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
            UserWordModel word, 
            UserWordModel[] examList)
        {
            if (!word.HasAnyExamples)
                return ExamResult.Impossible;
            
            var targetPhrase = word.GetRandomExample();

            var otherExamples = examList
                .SelectMany(e => e.Examples)
                .Where(p => !p.TranslatedPhrase.AreEqualIgnoreCase(targetPhrase.TranslatedPhrase))
                .Randomize()
                .Take(5)
                .ToArray();

            if(!otherExamples.Any())
                return ExamResult.Impossible;
            
            var variants = otherExamples
                .Append(targetPhrase)
                .Select(e => e.TranslatedPhrase)
                .Randomize()
                .ToArray();
            
            var msg = $"=====>   {targetPhrase.OriginPhrase}    <=====\r\n" +
                      $"Choose the translation";
            await chatIo.SendMessageAsync(msg, InlineButtons.CreateVariants(variants));
            
            var choice = await chatIo.TryWaitInlineIntKeyboardInput();
            if (choice == null)
                return ExamResult.Retry;
            
            return variants[choice.Value].AreEqualIgnoreCase(targetPhrase.TranslatedPhrase) 
                ? ExamResult.Passed 
                : ExamResult.Failed;
        }
    }
}