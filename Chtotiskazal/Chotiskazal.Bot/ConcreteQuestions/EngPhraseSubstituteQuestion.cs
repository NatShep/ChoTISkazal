using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chotiskazal.Bot.Interface;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions
{
    public class EngPhraseSubstituteQuestion: IQuestion
    {
        public bool NeedClearScreen => false;
        public string Name => "Eng phrase substitute";

        public async Task<QuestionResultMarkdown> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList) {
            var (phrase, translation) = word.GetExamplesThatLoadedAndFits().GetRandomItemOrNull();
            if (phrase == null)
                return QuestionResultMarkdown.Impossible;
            var (enPhrase, ruPhrase) = phrase.Deconstruct();
            
            var allWordsWithPhraseOfSimilarTranslate = examList
                                                       .SelectMany(e => e.Examples)
                                                       .Where(p => p.TranslatedPhrase.AreEqualIgnoreCase(ruPhrase))
                                                       .Select(e => e.OriginWord)
                                                       .ToList();

            var enReplaced = enPhrase.Replace(phrase.OriginWord, "...");
            if (enReplaced == enPhrase)
                return QuestionResultMarkdown.Impossible;

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
                return QuestionResultMarkdown.RetryThisQuestion;
            }
            
            var (closestWord, comparation) = allWordsWithPhraseOfSimilarTranslate.GetClosestTo(enter.Trim());
            if (comparation == StringsCompareResult.Equal)
                return QuestionResultMarkdown.Passed(chat.Texts);
            if (enter.Contains(word.Word, StringComparison.InvariantCultureIgnoreCase) && enPhrase.Contains(enter))
            {
                //if user enters whole world (as it is in phrase) - it is ok
                return QuestionResultMarkdown.Passed(chat.Texts);
            }
            
            if (comparation == StringsCompareResult.SmallMistakes)
            {
                await chat.SendMessageAsync(chat.Texts.TypoAlmostRight);
                return QuestionResultMarkdown.RetryThisQuestion;
            }

            return QuestionResultMarkdown.Failed(
                chat.Texts.FailedOriginExampleWasMarkdown.AddNewLine() +
                MarkdownObject.Escaped($"\"{phrase.OriginPhrase}\"").ToSemiBold(),
                chat.Texts);
        }
    }
}
