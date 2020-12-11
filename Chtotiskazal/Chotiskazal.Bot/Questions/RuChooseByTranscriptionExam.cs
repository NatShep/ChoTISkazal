using System.Linq;
using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.Questions
{
    public class RuChooseByTranscriptionExam:IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Ru Choose By Transcription";

        public async Task<ExamResult> Pass(ChatIO chatIo,  UserWordModel word, UserWordModel[] examList)
        {
            var originTranslation = word.Translations.GetRandomItem();
            
            if (string.IsNullOrWhiteSpace(originTranslation.Transcription) || originTranslation.Transcription!="")
                return ExamResult.Impossible;
            
            var variants = examList.Where(e=> e.Translations.All(t => t.Transcription != originTranslation.Transcription))
                .SelectMany(e => e.AllTranslations)
                .Take(5)
                .Append(originTranslation.Word)
                .Randomize()
                .ToList();


            var msg = $"=====>   {originTranslation.Transcription}    <=====\r\n" +
                      $"Choose which word has this transcription";
            await chatIo.SendMessageAsync(msg, InlineButtons.CreateVariants(variants));

            var choice = await chatIo.TryWaitInlineIntKeyboardInput();
            if (choice == null)
                return ExamResult.Retry;

            if (word.AllTranslations.Contains(variants[choice.Value]))
            {
                return ExamResult.Passed;
            }
            return ExamResult.Failed;

        }
        
    }
}