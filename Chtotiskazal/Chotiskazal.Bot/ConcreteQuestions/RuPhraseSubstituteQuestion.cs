using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chotiskazal.Bot.InterfaceLang;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions
{
    public class RuPhraseSubstituteQuestion : IQuestion
    {
        public bool NeedClearScreen => false;

        public string Name => "Ru phrase substitute";
        public async Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word,
            UserWordModel[] examList)
        {
            if (!word.Examples.Any())
                return QuestionResult.Impossible;

            var phrase = word.GetRandomExample();

            var replaced = phrase.TranslatedPhrase.Replace(phrase.TranslatedWord, "\\.\\.\\.");
            if (replaced == phrase.TranslatedPhrase)
                return QuestionResult.Impossible;

            var sb = new StringBuilder();
            
            sb.AppendLine($"*\"{phrase.OriginPhrase}\"*");
            sb.AppendLine($"    _{chat.Texts.translatesAs}_ ");
            sb.AppendLine($"*\"{replaced}\"*");
            sb.AppendLine();
            sb.AppendLine($"{chat.Texts.EnterMissingWord}: ");
            var msg = sb.ToString().Replace(".","\\.");
            await chat.SendMarkdownMessageAsync(sb.ToString());

            while (true)
            {
                var enter = await chat.WaitUserTextInputAsync();
                if (string.IsNullOrWhiteSpace(enter))
                    continue;
                var comparation = phrase.TranslatedWord.CheckForMistakes(enter.Trim());

                if (comparation== StringsCompareResult.Equal)
                    return QuestionResult.Passed(chat.Texts);

                if (comparation == StringsCompareResult.SmallMistakes) {
                    await chat.SendMessageAsync(chat.Texts.RetryAlmostRightWithTypo);
                    return QuestionResult.RetryThisQuestion;
                }
                return QuestionResult.Failed(
                    $"{chat.Texts.FailedOriginExampleWas2} *\"{phrase.TranslatedPhrase}\"*", 
                    chat.Texts);
            }
        }
    }
}