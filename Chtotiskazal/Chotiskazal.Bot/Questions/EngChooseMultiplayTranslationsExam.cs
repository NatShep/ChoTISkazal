using System;
using System.Linq;
using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.Questions
{
    public class EngChooseMultipleTranslationsExam:IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Eng Choose";

        public async Task<ExamResult> Pass(ChatIO chatIo,  UserWordModel word,
            UserWordModel[] examList)
        {
            var translations = word.TranslationAsList;
            
            var variants = examList
                .Where(e => e.TranslationAsList != word.TranslationAsList)
                .Select(e => e.TranslationAsList)
                .Randomize()
                .Take(5)
                .Append(translations)
                .ToList();

            var msg = $"=====>   {word.Word}    <=====\r\n" +
                      $"Choose the translation";
            await chatIo.SendMessageAsync(msg, InlineButtons.CreateVariants(variants));

            var choice = await chatIo.TryWaitInlineIntKeyboardInput();
            if (choice == null)
                return ExamResult.Retry;

            var answer = variants[choice.Value].Split(",")
                .Select(e => e.Trim()).ToList();
            
            if (!answer.Except(word.AllTranslations).Any())
            {
                return ExamResult.Passed;
            }
            return ExamResult.Failed;
        }
    }
}