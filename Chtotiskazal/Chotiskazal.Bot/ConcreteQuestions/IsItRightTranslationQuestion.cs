using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Interface;
using Chotiskazal.Bot.Questions;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.ConcreteQuestions
{
    public class IsItRightTranslationQuestion: IQuestion
    {
        public bool NeedClearScreen => false;
        public string Name => "Eng is it right translation";

        public async Task<QuestionResultMarkdown> Pass(ChatRoom chat, UserWordModel word,
            UserWordModel[] examList)
        {
            var translation = examList.SelectMany(e => e.TextTranslations)
                .Where(e => word.RuTranslations.All(t => t.Word != e))
                .Shuffle()
                .Take(1)
                .Union(word.TextTranslations)
                .ToList()
                .GetRandomItemOrNull();

            var msg = QuestionMarkups.TranslatesAsTemplate(
                    word.Word,
                    chat.Texts.translatesAs,
                    translation,
                    chat.Texts.IsItRightTranslation);

            await chat.SendMarkdownMessageAsync(msg,
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
            if  (
                choice == 1 &&  word.TextTranslations.Contains(translation) ||
                choice == 0 && !word.TextTranslations.Contains(translation)
                )
            {
                return QuestionResultMarkdown.Passed(chat.Texts);
            }
            else {
                return QuestionResultMarkdown.Failed(
                    MarkdownObject.Escaped($"{chat.Texts.Mistaken}.").AddNewLine() +
                    MarkdownObject.Escaped($"\"{word.Word}\" {chat.Texts.translatesAs} ") +
                    MarkdownObject.Escaped($"\"{word.TextTranslations.FirstOrDefault()}\" ").ToSemiBold(), 
                    chat.Texts);
            }
        }
    }
}