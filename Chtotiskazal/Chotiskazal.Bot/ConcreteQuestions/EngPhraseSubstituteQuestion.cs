using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chotiskazal.Bot.InterfaceLang;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions
{
    public class EngPhraseSubstituteQuestion: IQuestion
    {
        public bool NeedClearScreen => false;
        public string Name => "Eng phrase substitute";
        public async Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList)
        {
            if (!word.Examples.Any())
                return QuestionResult.Impossible;

            var phrase   =  word.GetRandomExample();
            
            var allWordsWithPhraseOfSimilarTranslate = examList
                .SelectMany(e => e.Examples)
                .Where(p => p.TranslatedPhrase.AreEqualIgnoreCase(phrase.TranslatedPhrase))
                .Select(e=>e.OriginWord)
                .ToList();
            
            var replaced =  phrase.OriginPhrase.Replace(phrase.OriginWord, "...");
            if (replaced == phrase.OriginPhrase)
                return QuestionResult.Impossible;
            
            var sb = new StringBuilder();
            
            sb.AppendLine($"\"{phrase.TranslatedPhrase}\"");
            sb.AppendLine($" {chat.Texts.translatesAs} ");
            sb.AppendLine($"\"{replaced}\"");
            sb.AppendLine();
            sb.AppendLine($"{chat.Texts.EnterMissingWord}: ");
            await chat.SendMessageAsync(sb.ToString());
            while (true)
            {
                var enter = await chat.WaitUserTextInputAsync();
                if (string.IsNullOrWhiteSpace(enter))
                    continue;
                var (_, comparation) = allWordsWithPhraseOfSimilarTranslate.GetClosestTo(enter.Trim());
                if (comparation == StringsCompareResult.Equal)
                    return QuestionResult.Passed(chat.Texts);
                if (comparation == StringsCompareResult.SmallMistakes) {
                    await chat.SendMessageAsync(chat.Texts.TypoAlmostRight);
                    return QuestionResult.RetryThisQuestion;
                }
                return QuestionResult.Failed($"{chat.Texts.FailedOriginExampleWas} \"{phrase.OriginPhrase}\"", 
                    chat.Texts);
            }
        }
    }
}
