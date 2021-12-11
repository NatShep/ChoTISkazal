using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Interface;
using Chotiskazal.Bot.Questions;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions
{
    public class TranscriptionChooseQuestion : IQuestion
    {
        public bool NeedClearScreen => false;

        public string Name => "Trans Choose";

        public async Task<QuestionResultMarkdown> Pass(ChatRoom chat, UserWordModel word,
            UserWordModel[] examList)
        {
            var originalTranslation = word.RuTranslations.GetRandomItemOrNull();

            if (originalTranslation==null || !originalTranslation.HasTranscription)
                return QuestionResultMarkdown.Impossible;
            
            var variants = examList
                .SelectMany(e => e.RuTranslations)
                .Select(e=>e.Transcription)
                .Where(e => word.RuTranslations.All(t => t.Transcription != e))
                .Distinct()
                .Shuffle()
                .Take(5)
                .Append(originalTranslation.Transcription)
                .Where(w=>w!=null)
                .Shuffle()
                .ToList();
            
            if (variants.Count <= 1)
                return QuestionResultMarkdown.Impossible;

            var msg = QuestionMarkups.TranslateTemplate(word.Word, chat.Texts.ChooseTheTranscription);
            await chat.SendMarkdownMessageAsync(msg, InlineButtons.CreateVariants(variants));

            var choice = await chat.TryWaitInlineIntKeyboardInput();
            if (choice == null)
                return QuestionResultMarkdown.RetryThisQuestion;

            return word.RuTranslations.Any(t=>t.Transcription== variants[choice.Value]) 
                ? QuestionResultMarkdown.Passed(chat.Texts) 
                : QuestionResultMarkdown.Failed(chat.Texts);
        }
    }
}