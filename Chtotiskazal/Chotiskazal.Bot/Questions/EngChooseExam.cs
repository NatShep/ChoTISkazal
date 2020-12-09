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
    public class EngChooseExam:IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Eng Choose";

        public async Task<ExamResult> Pass(ChatIO chatIo, UsersWordsService service, UserWordModel word,
            UserWordModel[] examList)
        {
            var variants = examList.SelectMany(e => e.AllTranslations)
                .Where(e => !word.AllTranslations.ToList().Contains(e))
                .Union(word.AllTranslations)
                .Randomize()
                .ToList();
          

            var msg = $"=====>   {word.Word}    <=====\r\n" +
                      $"Choose the translation";
            await chatIo.SendMessageAsync(msg, InlineButtons.CreateVariants(variants));

            var choice = await chatIo.TryWaitInlineIntKeyboardInput();
            if (choice == null)
                return ExamResult.Retry;

            var selected = variants[choice.Value];
            return word.AllTranslations.Any(t=>string.Equals(t,selected, StringComparison.InvariantCultureIgnoreCase)) 
                ? ExamResult.Passed 
                : ExamResult.Failed;
        }
    }
}