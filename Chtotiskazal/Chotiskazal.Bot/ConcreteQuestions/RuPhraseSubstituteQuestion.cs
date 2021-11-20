using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions
{
    public class RuPhraseSubstituteQuestion : IQuestion
    {
        public bool NeedClearScreen => false;

        public string Name => "Ru phrase substitute";

        public async Task<QuestionResult> Pass(
            ChatRoom chat, UserWordModel word,
            UserWordModel[] examList) {
            var (phrase, translation) = word.GetExamplesThatLoadedAndFits().GetRandomItemOrNull();
            if (phrase == null)
                return QuestionResult.Impossible;
            var (enPhrase,ruPhrase) = phrase.Deconstruct();
            var replacedRuPhrase = ruPhrase.Replace(phrase.TranslatedWord, "\\.\\.\\.");
            if (replacedRuPhrase == ruPhrase)
                return QuestionResult.Impossible;

            var sb = new StringBuilder();
            sb.AppendLine($"*\"{enPhrase}\"*");
            sb.AppendLine($"    _{chat.Texts.translatesAs}_ ");
            sb.AppendLine($"*\"{replacedRuPhrase}\"*");
            sb.AppendLine();
            sb.AppendLine($"{chat.Texts.EnterMissingWord}: ");
            await chat.SendMarkdownMessageAsync(sb.ToString());

            var enter = await chat.WaitNonEmptyUserTextInputAsync();
            
            if (!enter.IsRussian())
            {
                await chat.SendMessageAsync(chat.Texts.RussianEnterExpected);
                return QuestionResult.RetryThisQuestion;
            }
            
            var comparation = translation.Word.CheckCloseness(enter.Trim());
            
            if (comparation == StringsCompareResult.Equal)
                return QuestionResult.Passed(chat.Texts);

            //if user enters whole word in phrase- it is ok!
            if (enter.Contains(translation.Word, StringComparison.InvariantCultureIgnoreCase) 
                && phrase.OriginPhrase.Contains(enter, StringComparison.InvariantCultureIgnoreCase))
                return QuestionResult.Passed(chat.Texts);

            if (comparation == StringsCompareResult.SmallMistakes)
            {
                await chat.SendMessageAsync(chat.Texts.RetryAlmostRightWithTypo);
                return QuestionResult.RetryThisQuestion;
            }

            
            return QuestionResult.Failed($"{chat.Texts.FailedOriginExampleWas2} *\"{phrase.TranslatedPhrase}\"*",
                chat.Texts);
        }
    }
}