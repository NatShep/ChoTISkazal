﻿using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions
{
    public class RuChooseQuestion: IQuestion
    {
        public bool NeedClearScreen => false;

        public string Name => "RuChoose";

        public async Task<QuestionResultMarkdown> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList)
        {
            var variants = examList.Where(e=>e.AllTranslationsAsSingleString!=word.AllTranslationsAsSingleString)
                .Select(e => e.Word)
                .Take(5)
                .Append(word.Word)
                .Shuffle()
                .ToArray();


            var msg = QuestionMarkups.TranslateTemplate(word.AllTranslationsAsSingleString, chat.Texts.ChooseTheTranslation);
            await chat.SendMarkdownMessageAsync(msg, InlineButtons.CreateVariants(variants));
            
            var choice = await chat.TryWaitInlineIntKeyboardInput();
            if (choice == null)
                return QuestionResultMarkdown.RetryThisQuestion;
            
            return variants[choice.Value].AreEqualIgnoreCase(word.Word) 
                ? QuestionResultMarkdown.Passed(chat.Texts) 
                : QuestionResultMarkdown.Failed(chat.Texts);
        }
    }
}