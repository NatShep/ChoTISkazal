using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.ConcreteQuestions
{
    public class RuChoosePhraseQuestion : IQuestion
    {
        public bool NeedClearScreen => false;

        public string Name => "Ru Choose Phrase";

        public async Task<QuestionResultMarkdown> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList)
        {
            if (!word.Examples.Any())
                return QuestionResultMarkdown.Impossible;
            
            var targetPhrase = word.GetRandomExample();

            var other = examList.SelectMany(e => e.Examples)
                .Where(p => !string.IsNullOrWhiteSpace(p?.OriginPhrase) && p.TranslatedPhrase!= targetPhrase.TranslatedPhrase)
                .Shuffle()
                .Take(5)
                .ToArray();

            if(!other.Any())
                return QuestionResultMarkdown.Impossible;

            var variants = other
                .Append(targetPhrase)
                .Shuffle()
                .Select(e => e.OriginPhrase)
                .ToArray();

            var msg = QuestionMarkups.TranslateTemplate(targetPhrase.TranslatedPhrase, chat.Texts.ChooseTheTranslation);
            await chat.SendMarkdownMessageAsync(msg,
                variants.Select((v, i) => new InlineKeyboardButton
                {
                    CallbackData = i.ToString(),
                    Text = v
                }).ToArray());
            
            var choice = await chat.TryWaitInlineIntKeyboardInput();
            if (choice == null)
                return QuestionResultMarkdown.RetryThisQuestion;
            
            return variants[choice.Value].AreEqualIgnoreCase(targetPhrase.OriginPhrase) 
                ? QuestionResultMarkdown.Passed(chat.Texts) 
                : QuestionResultMarkdown.Failed(chat.Texts);
        }
    }
}