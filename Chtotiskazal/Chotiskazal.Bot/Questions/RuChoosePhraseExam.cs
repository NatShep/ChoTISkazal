using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Services;
using Chotiskazal.Dal.DAL;
using Chotiskazal.DAL.Services;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.Questions
{
    public class RuChoosePhraseExam : IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Ru Choose Phrase";

        public async Task<ExamResult> Pass(ChatIO chatIo, ExamService service, UserWordForLearning word, UserWordForLearning[] examList)
        {
            if (!word.Phrases.Any())
                return ExamResult.Impossible;
            
            var targetPhrase = word.Phrases.GetRandomItem();

            var other = examList.SelectMany(e => e.Phrases)
                .Where(p => !string.IsNullOrWhiteSpace(p?.EnPhrase) && p!= targetPhrase)
                .Take(8).ToArray();

            if(!other.Any())
                return ExamResult.Impossible;

            var variants = other
                .Append(targetPhrase)
                .Randomize()
                .Select(e => e.EnPhrase)
                .ToArray();
            
            var msg = $"=====>   {targetPhrase.PhraseRuTranslate}    <=====\r\nChoose the translation";
            await chatIo.SendMessageAsync(msg,
                variants.Select((v, i) => new InlineKeyboardButton
                {
                    CallbackData = i.ToString(),
                    Text = v
                }).ToArray());
            
            var choice = await chatIo.TryWaitInlineIntKeyboardInputAsync();
            if (choice == null)
                return ExamResult.Retry;
            
            if (variants[choice.Value] == targetPhrase.EnPhrase)
            {
                await service.RegisterSuccessAsync(word);
                return ExamResult.Passed;
            }
            await service.RegisterFailureAsync(word);
            return ExamResult.Failed;
        }
    }
}