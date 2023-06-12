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
    public class EngPhraseSubstituteQuestion: IQuestion
    {
        public bool NeedClearScreen => false;
        public string Name => "Eng phrase substitute";
        public double PassScore => 0.6;
        public double FailScore => 0.6;

        public async Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList) {
            var (phrase, translation) = word.GetExamplesThatLoadedAndFits().GetRandomItemOrNull();
            if (phrase == null)
                return QuestionResult.Impossible;
            var (enPhrase, ruPhrase) = phrase.Deconstruct();
            
            var allWordsWithPhraseOfSimilarTranslate = examList
                                                       .SelectMany(e => e.Examples)
                                                       .Where(p => p.TranslatedPhrase.AreEqualIgnoreCase(ruPhrase))
                                                       .Select(e => e.OriginWord)
                                                       .ToList();

            var enReplaced = enPhrase.Replace(phrase.OriginWord, "...");
            if (enReplaced == enPhrase)
                return QuestionResult.Impossible;

            await chat.SendMarkdownMessageAsync(
                QuestionMarkups.TranslatesAsTemplate(
                    ruPhrase, 
                    chat.Texts.translatesAs, 
                    enReplaced, 
                    chat.Texts.EnterMissingWord+":"));

            var enter = await chat.WaitNonEmptyUserTextInputAsync();
            if (enter.IsRussian())
            {
                await chat.SendMessageAsync(chat.Texts.EnglishInputExpected);
                return QuestionResult.RetryThisQuestion;
            }
            
            var (closestWord, comparation) = allWordsWithPhraseOfSimilarTranslate.GetClosestTo(enter.Trim());
            if (comparation == StringsCompareResult.Equal)
                return QuestionResult.Passed(chat.Texts);
            if (enter.Contains(word.Word, StringComparison.InvariantCultureIgnoreCase) && enPhrase.Contains(enter))
            {
                //if user enters whole world (as it is in phrase) - it is ok
                return QuestionResult.Passed(chat.Texts);
            }
            
            if (comparation == StringsCompareResult.SmallMistakes)
            {
                await chat.SendMessageAsync(chat.Texts.TypoAlmostRight);
                return QuestionResult.RetryThisQuestion;
            }
            //if user enters whole phrase - it is ok
            var phraseCloseness = enPhrase.CheckCloseness(enter.Trim());
            if(phraseCloseness == StringsCompareResult.Equal)
                return QuestionResult.Passed(chat.Texts);
            if(phraseCloseness == StringsCompareResult.SmallMistakes)
            {
                await chat.SendMessageAsync(chat.Texts.TypoAlmostRight);
                return QuestionResult.RetryThisQuestion;
            }
            
            return QuestionResult.Failed(
                chat.Texts.FailedOriginExampleWas.NewLine() +
                Markdown.Escaped($"\"{phrase.OriginPhrase}\"").ToSemiBold(),
                chat.Texts);
        }
    }
}
