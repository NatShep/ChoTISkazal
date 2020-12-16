﻿using System;
using System.Linq;
using System.Threading.Tasks;
using SayWhat.Bll;
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
                .Take(5)
                .Append(word.Word)
                .Randomize()
                .ToArray();

            var msg = $"=====>   {word.TranslationAsList}    <=====\r\n" +
                      $"Choose the translation";
            await chatIo.SendMessageAsync(msg, InlineButtons.CreateVariants(variants));
            
            var choice = await chatIo.TryWaitInlineIntKeyboardInput();
            if (choice == null)
                return ExamResult.Retry;
            
            return variants[choice.Value].AreEqualIgnoreCase(word.Word) 
                ? ExamResult.Passed 
                : ExamResult.Failed;
        }
    }
}