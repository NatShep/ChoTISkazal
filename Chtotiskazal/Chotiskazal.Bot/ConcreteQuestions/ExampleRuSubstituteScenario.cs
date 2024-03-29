﻿using System;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions;

public class ExampleRuSubstituteScenario : IQuestionScenario {
    public QuestionInputType InputType => QuestionInputType.NeedsRuInput;
    public ScenarioWordTypeFit Fit => ScenarioWordTypeFit.OnlyWord;

    public async Task<QuestionResult> Pass(
        ChatRoom chat, UserWordModel word,
        UserWordModel[] examList) {
        var (phrase, translation) = word.GetExamplesThatLoadedAndFits().GetRandomItemOrNull();
        if (phrase == null)
            return QuestionResult.Impossible;
        var (enPhrase, ruPhrase) = phrase.Deconstruct();
        var replacedRuPhrase = ruPhrase.Replace(phrase.TranslatedWord, "...");
        if (replacedRuPhrase == ruPhrase)
            return QuestionResult.Impossible;

        var (result, enter) = await QuestionScenarioHelper.GetRussianUserInputOrIDontKnow(chat,
            QuestionMarkups.TranslatesAsTemplate(
                enPhrase, chat.Texts.translatesAs, replacedRuPhrase, chat.Texts.EnterMissingWord + ":"));
        if (result == OptionalUserInputResult.IDontKnow)
            return Failed();
        if (result == OptionalUserInputResult.NotAnInput)
            return QuestionResult.RetryThisQuestion;

        var comparation = translation.Word.CheckCloseness(enter.Trim());

        if (comparation == StringsCompareResult.Equal)
            return QuestionResult.Passed(chat.Texts);

        //if user enters whole word in phrase- it is ok!
        if (enter.Contains(translation.Word, StringComparison.InvariantCultureIgnoreCase)
            && phrase.OriginPhrase.Contains(enter, StringComparison.InvariantCultureIgnoreCase))
            return QuestionResult.Passed(chat.Texts);

        if (comparation == StringsCompareResult.SmallMistakes) {
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

        return Failed();

        QuestionResult Failed() => QuestionResult.Failed(
                chat.Texts.FailedOriginPhraseWas2 +
                Markdown.Escaped($"\"{phrase.TranslatedPhrase}\"").ToSemiBold(), chat.Texts);
    }
}