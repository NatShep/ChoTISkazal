using System.Linq;
using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.Bll.Dto;
using SayWhat.Bll.Services;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.Questions
{
    public class RuChoosePhraseExam : IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Ru Choose Phrase";

        public async Task<ExamResult> Pass(ChatIO chatIo, UsersWordsService service, UserWordModel word, UserWordModel[] examList)
        {
            if (!word.Phrases.Any())
                return ExamResult.Impossible;
            
            var targetPhrase = word.GetRandomExample();

            var other = examList.SelectMany(e => e.Phrases)
                .Where(p => !string.IsNullOrWhiteSpace(p?.OriginPhrase) && p!= targetPhrase)
                .Take(8).ToArray();

            if(!other.Any())
                return ExamResult.Impossible;

            var variants = other
                .Append(targetPhrase)
                .Randomize()
                .Select(e => e.OriginPhrase)
                .ToArray();
            
            var msg = $"=====>   {targetPhrase.TranslatedPhrase}    <=====\r\n" +
                      $"Choose the translation";
            await chatIo.SendMessageAsync(msg,
                variants.Select((v, i) => new InlineKeyboardButton
                {
                    CallbackData = i.ToString(),
                    Text = v
                }).ToArray());
            
            var choice = await chatIo.TryWaitInlineIntKeyboardInput();
            if (choice == null)
                return ExamResult.Retry;
            
            if (variants[choice.Value] == targetPhrase.OriginPhrase)
            {
                await service.RegisterSuccess(word);
                return ExamResult.Passed;
            }
            await service.RegisterFailure(word);
            return ExamResult.Failed;
        }
    }
}