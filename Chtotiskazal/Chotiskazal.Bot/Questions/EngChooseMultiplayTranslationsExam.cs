using System;
using System.Linq;
using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.Bll.Services;

namespace Chotiskazal.Bot.Questions
{
    public class EngChooseMultipleTranslationsExam:IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Eng Choose";

        public async Task<ExamResult> Pass(ChatIO chatIo, UsersWordsService service, UserWordModel word,
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
            
            if (!answer.Except(word.GetTranslations()).Any())
            {
                await service.RegisterSuccess(word);
                return ExamResult.Passed;
            }

            await service.RegisterFailure(word);
            return ExamResult.Failed;
        }
    }
}