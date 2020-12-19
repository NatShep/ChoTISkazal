using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.InterfaceLang;
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

        public async Task<QuestionResult> Pass(ChatIO chatIo, UserWordModel word, UserWordModel[] examList)
        {
            if (!word.Examples.Any())
                return QuestionResult.Impossible;
            
            var targetPhrase = word.GetRandomExample();

            var other = examList.SelectMany(e => e.Examples)
                .Where(p => !string.IsNullOrWhiteSpace(p?.OriginPhrase) && p.TranslatedPhrase!= targetPhrase.TranslatedPhrase)
                .Randomize()
                .Take(5)
                .ToArray();

            if(!other.Any())
                return QuestionResult.Impossible;

            var variants = other
                .Append(targetPhrase)
                .Randomize()
                .Select(e => e.OriginPhrase)
                .ToArray();
            
            var msg = $"=====>   {targetPhrase.TranslatedPhrase}    <=====\r\n" +
                      Texts.Current.ChooseTheTranslation;
            await chatIo.SendMessageAsync(msg,
                variants.Select((v, i) => new InlineKeyboardButton
                {
                    CallbackData = i.ToString(),
                    Text = v
                }).ToArray());
            
            var choice = await chatIo.TryWaitInlineIntKeyboardInput();
            if (choice == null)
                return QuestionResult.Retry;
            
            return variants[choice.Value].AreEqualIgnoreCase(targetPhrase.OriginPhrase) 
                ? QuestionResult.Passed 
                : QuestionResult.Failed;
        }
    }
}