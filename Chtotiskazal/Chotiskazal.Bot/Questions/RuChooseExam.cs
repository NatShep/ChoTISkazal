using System;
using System.Linq;
using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.Bll.Dto;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.Questions
{
    public class RuChooseExam: IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "RuChoose";

        public async Task<ExamResult> Pass(ChatIO chatIo, UserWordModel word, UserWordModel[] examList)
        {
            var variants = examList.Where(e=>e.TranslationAsList!=word.TranslationAsList)
                .Select(e => e.Word)
                .Append(word.Word)
                .Where(e => word.TranslationAsList!=e)
                .Randomize()
                .ToArray();

            var msg = $"=====>   {word.TranslationAsList}    <=====\r\n" +
                      $"Choose the translation";
            await chatIo.SendMessageAsync(msg, InlineButtons.CreateVariants(variants));
            
            var choice = await chatIo.TryWaitInlineIntKeyboardInput();
            if (choice == null)
                return ExamResult.Retry;
            
            return string.Equals(variants[choice.Value],word.Word, StringComparison.InvariantCultureIgnoreCase) 
                ? ExamResult.Passed 
                : ExamResult.Failed;
        }
    }
}