using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions
{
    public class RuChooseByTranscriptionQuestion:IQuestion
    {
        public bool NeedClearScreen => false;

        public string Name => "Ru Choose By Transcription";

        public async Task<QuestionResultMarkdown> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList)
        {
            var originTranslation = word.RuTranslations.GetRandomItemOrNull();
            
            if (string.IsNullOrWhiteSpace(originTranslation.Transcription) || originTranslation.Transcription!="")
                return QuestionResultMarkdown.Impossible;
            
            var variants = examList.Where(e=> e.RuTranslations.All(t => t.Transcription != originTranslation.Transcription))
                .SelectMany(e => e.TextTranslations)
                .Take(5)
                .Append(originTranslation.Word)
                .Shuffle()
                .ToList();


            var msg = QuestionMarkups.TranscriptionTemplate(originTranslation.Transcription, chat.Texts.ChooseWhichWordHasThisTranscription);
            await chat.SendMarkdownMessageAsync(msg, InlineButtons.CreateVariants(variants));

            var choice = await chat.TryWaitInlineIntKeyboardInput();
            if (choice == null)
                return QuestionResultMarkdown.RetryThisQuestion;

            if (word.TextTranslations.Contains(variants[choice.Value]))
                return QuestionResultMarkdown.Passed(chat.Texts);
            return QuestionResultMarkdown.Failed(chat.Texts);

        }
        
    }
}