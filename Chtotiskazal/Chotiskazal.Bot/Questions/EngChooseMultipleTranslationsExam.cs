using System;
using System.ComponentModel.Design;
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

        public async Task<QuestionResult> Pass(ChatIO chatIo,  UserWordModel word,
            UserWordModel[] examList)
        {
            var translations = word.AllTranslations.Randomize().Take(3);
            
            var variants = examList
                .Where(e => e.TranslationAsList != word.TranslationAsList)
                .SelectMany(e => e.AllTranslations.Randomize().Take(3))
                .Randomize()
                .Take(5)
                .Union(translations)
                .ToList();

            var msg = $"=====>   {word.Word}    <=====\r\n" +
                      $"Choose the translation";
            await chatIo.SendMessageAsync(msg, InlineButtons.CreateVariants(variants));

            var choice = await chatIo.TryWaitInlineIntKeyboardInput();
            if (choice == null)
                return QuestionResult.Retry;

            var answer = variants[choice.Value].Split(",")
                .Select(e => e.Trim()).ToList();
            
            return !answer.Except(word.AllTranslations).Any() 
                ? QuestionResult.Passed 
                : QuestionResult.Failed;
        }
    }
}