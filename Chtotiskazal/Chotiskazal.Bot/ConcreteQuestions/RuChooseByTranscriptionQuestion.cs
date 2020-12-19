using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.InterfaceLang;
using Chotiskazal.Bot.Questions;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions
{
    public class RuChooseByTranscriptionQuestion:IQuestion
    {
        public bool NeedClearScreen => false;

        public string Name => "Ru Choose By Transcription";

        public async Task<QuestionResult> Pass(ChatIO chatIo,  UserWordModel word, UserWordModel[] examList)
        {
            var originTranslation = word.Translations.GetRandomItem();
            
            if (string.IsNullOrWhiteSpace(originTranslation.Transcription) || originTranslation.Transcription!="")
                return QuestionResult.Impossible;
            
            var variants = examList.Where(e=> e.Translations.All(t => t.Transcription != originTranslation.Transcription))
                .SelectMany(e => e.TextTranslations)
                .Take(5)
                .Append(originTranslation.Word)
                .Randomize()
                .ToList();


            var msg = $"=====>   {originTranslation.Transcription}    <=====\r\n" +
                      Texts.Current.ChooseWhichWordHasThisTranscription;
            await chatIo.SendMessageAsync(msg, InlineButtons.CreateVariants(variants));

            var choice = await chatIo.TryWaitInlineIntKeyboardInput();
            if (choice == null)
                return QuestionResult.Retry;

            if (word.TextTranslations.Contains(variants[choice.Value]))
                return QuestionResult.Passed;
            return QuestionResult.Failed;

        }
        
    }
}