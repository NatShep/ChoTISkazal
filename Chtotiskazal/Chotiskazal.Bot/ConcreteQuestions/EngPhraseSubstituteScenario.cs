using System;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions;

public class EngPhraseSubstituteScenario : IQuestionScenario {
    public QuestionInputType InputType => QuestionInputType.NeedsEnInput;

    public async Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList) {
        var (phrase, _) = word.GetExamplesThatLoadedAndFits().GetRandomItemOrNull();
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

        var (result, enter) = await QuestionScenarioHelper.GetEnglishUserInputOrIDontKnow(chat,
            QuestionMarkups.TranslatesAsTemplate(
                ruPhrase,
                chat.Texts.translatesAs,
                enReplaced,
                chat.Texts.EnterMissingWord + ":"));
        
        if(result == OptionalUserInputResult.IDontKnow)
            return QuestionResult.Failed(Markdown.Empty, Markdown.Empty);
        if(result== OptionalUserInputResult.NotAnInput)
            return QuestionResult.RetryThisQuestion;
        

        var (closestWord, comparation) = allWordsWithPhraseOfSimilarTranslate.GetClosestTo(enter.Trim());
        if (comparation == StringsCompareResult.Equal)
            return QuestionResult.Passed(chat.Texts);
        if (enter.Contains(word.Word, StringComparison.InvariantCultureIgnoreCase) && enPhrase.Contains(enter)) {
            //if user enters whole world (as it is in phrase) - it is ok
            return QuestionResult.Passed(chat.Texts);
        }

        if (comparation == StringsCompareResult.SmallMistakes) {
            await chat.SendMessageAsync(chat.Texts.TypoAlmostRight);
            return QuestionResult.RetryThisQuestion;
        }

        //if user enters whole phrase - it is ok
        var phraseCloseness = enPhrase.CheckCloseness(enter.Trim());
        if (phraseCloseness == StringsCompareResult.Equal)
            return QuestionResult.Passed(chat.Texts);
        if (phraseCloseness == StringsCompareResult.SmallMistakes) {
            await chat.SendMessageAsync(chat.Texts.TypoAlmostRight);
            return QuestionResult.RetryThisQuestion;
        }

        return QuestionResult.Failed(
            chat.Texts.FailedOriginExampleWas.NewLine() +
            Markdown.Escaped($"\"{phrase.OriginPhrase}\"").ToSemiBold(),
            chat.Texts);
    }
}