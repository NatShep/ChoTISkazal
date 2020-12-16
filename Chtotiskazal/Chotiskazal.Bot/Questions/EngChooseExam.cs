using System;
using System.Linq;
using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.Questions
{
    public class EngChooseExam:IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Eng Choose";

        public async Task<ExamResult> Pass(ChatIO chatIo, UserWordModel word,
            UserWordModel[] examList)
        {
            var originTranslation = word.Translations.ToList().GetRandomItem();
           
            var variants = examList.SelectMany(e => e.AllTranslations)
                .Where(e => !word.AllTranslations.Contains(e))
                .Randomize()
                .Take(6-1)
                .Append(originTranslation.Word)
                .Randomize()
                .ToList();

            var msg = $"=====>   {word.Word}    <=====\r\n" +
                      $"Choose the translation";
            
            await chatIo.SendMessageAsync(msg, InlineButtons.CreateVariants(variants));

            var choice = await chatIo.TryWaitInlineIntKeyboardInput();
            if (choice == null)
                return ExamResult.Retry;

            var selected = variants[choice.Value];
            return word.AllTranslations.Any(t=>t.AreEqualIgnoreCase(selected)) 
                ? ExamResult.Passed 
                : ExamResult.Failed;
        }
    }
}