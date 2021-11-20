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

        public async Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList)
        {
            var originTranslation = word.Translations.GetRandomItemOrNull();
            
            if (string.IsNullOrWhiteSpace(originTranslation.Transcription) || originTranslation.Transcription!="")
                return QuestionResult.Impossible;
            
            var variants = examList.Where(e=> e.Translations.All(t => t.Transcription != originTranslation.Transcription))
                .SelectMany(e => e.TextTranslations)
                .Take(5)
                .Append(originTranslation.Word)
                .Shuffle()
                .ToList();


            var msg = $"\\=\\=\\=\\=\\=\\>   `{originTranslation.Transcription}`    \\<\\=\\=\\=\\=\\=\r\n" +
                      ""+chat.Texts.ChooseWhichWordHasThisTranscription+"";
            await chat.SendMarkdownMessageAsync(msg, InlineButtons.CreateVariants(variants));

            var choice = await chat.TryWaitInlineIntKeyboardInput();
            if (choice == null)
                return QuestionResult.RetryThisQuestion;

            if (word.TextTranslations.Contains(variants[choice.Value]))
                return QuestionResult.Passed(chat.Texts);
            return QuestionResult.Failed(chat.Texts);

        }
        
    }
}