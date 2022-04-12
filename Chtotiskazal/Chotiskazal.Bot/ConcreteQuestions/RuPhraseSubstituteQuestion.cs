using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chotiskazal.Bot.Interface;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll;
using SayWhat.Bll.Strings;
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
            var replacedRuPhrase = ruPhrase.Replace(phrase.TranslatedWord, "...");
            if (replacedRuPhrase == ruPhrase)
                return QuestionResult.Impossible;

            await chat.SendMarkdownMessageAsync(
                    QuestionMarkups.TranslatesAsTemplate(
                        enPhrase,
                        chat.Texts.translatesAs,
                        replacedRuPhrase,
                        chat.Texts.EnterMissingWord+":"));

            var enter = await chat.WaitNonEmptyUserTextInputAsync();
            
            if (!enter.IsRussian())
            {
                await chat.SendMessageAsync(chat.Texts.RussianInputExpected);
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
            //if user enters whole phrase - it is ok!
            
            var phraseComparation = ruPhrase.CheckCloseness(enter.Trim());
            if (phraseComparation == StringsCompareResult.Equal)
                return QuestionResult.Passed(chat.Texts);
            if (phraseComparation == StringsCompareResult.SmallMistakes) {
                await chat.SendMessageAsync(chat.Texts.RetryAlmostRightWithTypo);
                return QuestionResult.RetryThisQuestion;
            }

            return QuestionResult.Failed(
                chat.Texts.FailedOriginExampleWas2 +
                Markdown.Escaped($"\"{phrase.TranslatedPhrase}\"").ToSemiBold(), chat.Texts);
        }
    }
}
