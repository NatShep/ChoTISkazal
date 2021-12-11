﻿using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Interface;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.ConcreteQuestions
{
    public class EnTrustQuestion : IQuestion
    {
        public bool NeedClearScreen => false;
        public string Name => "Eng trust";

        public async Task<QuestionResultMarkdown> Pass(ChatRoom chat, UserWordModel word,
            UserWordModel[] examList) {
            var msg = QuestionMarkups.TranslateTemplate(word.Word, chat.Texts.DoYouKnowTranslation);
            var id = Rand.Next();
            await chat.SendMarkdownMessageAsync(msg,
                new InlineKeyboardButton {
                    CallbackData = id.ToString(),
                    Text =  chat.Texts.SeeTheTranslation 
                });
            while (true)
            {
                var update = await chat.WaitUserInputAsync();
                if(update.CallbackQuery?.Data== id.ToString())
                    break;
                var input = update.Message?.Text;
                if (!string.IsNullOrWhiteSpace(input))
                {
                    if (word.TextTranslations.Any(a =>
                        input.AreEqualIgnoreCase(a)))
                        return QuestionResultMarkdown.Passed(chat.Texts);

                    await chat.SendMessageAsync(chat.Texts.ItIsNotRightTryAgain);
                }
            }

            await chat.SendMarkdownMessageAsync(MarkdownObject.Escaped(chat.Texts.TranslationIs).ToItalic()
                    .AddNewLine() +
                                                MarkdownObject.Escaped($"\"{word.AllTranslationsAsSingleString}\"")
                                                    .ToSemiBold()
                                                    .AddNewLine()
                                                    .AddNewLine() +
                                                MarkdownObject.Escaped(chat.Texts.DidYouGuess),
                                            new[] {
                                                new[] {
                                                    new InlineKeyboardButton {
                                                        CallbackData = "1",
                                                        Text = chat.Texts.YesButton
                                                    },
                                                    new InlineKeyboardButton {
                                                        CallbackData = "0",
                                                        Text = chat.Texts.NoButton
                                                    }
                                                }
                                            });

            var choice = await chat.WaitInlineIntKeyboardInput();

            return choice == 1 ? 
                QuestionResultMarkdown.Passed(MarkdownObject.Escaped(chat.Texts.PassedOpenIHopeYouWereHonest), 
                    MarkdownObject.Escaped(chat.Texts.PassedHideousWell)) 
                : QuestionResultMarkdown.Failed(MarkdownObject.Escaped(chat.Texts.FailedOpenButYouWereHonest), 
                    MarkdownObject.Escaped(chat.Texts.FailedHideousHonestyIsGold));
        }
    }
}