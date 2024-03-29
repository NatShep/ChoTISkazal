﻿using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions;

public class ExampleRuChooseScenario : IQuestionScenario {
    public QuestionInputType InputType => QuestionInputType.NeedsNoInput;
    public ScenarioWordTypeFit Fit => ScenarioWordTypeFit.OnlyWord;

    public async Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList) {
        if (!word.Examples.Any())
            return QuestionResult.Impossible;

        var targetPhrase = word.GetRandomExample();

        var other = examList
            .Where(e=>e!= word)
            .SelectMany(e => e.Examples)
            .Where(p => !string.IsNullOrWhiteSpace(p?.OriginPhrase) &&
                        p.TranslatedPhrase != targetPhrase.TranslatedPhrase)
            .Shuffle()
            .Take(5)
            .ToArray();

        if (!other.Any())
            return QuestionResult.Impossible;

        var variants = other
            .Append(targetPhrase)
            .Select(e => e.OriginPhrase)
            .Shuffle()
            .ToArray();

        var choice = await QuestionScenarioHelper.ChooseVariantsFlow(chat, targetPhrase.TranslatedPhrase, variants);
        if (choice == null)
            return QuestionResult.RetryThisQuestion;

        return choice.AreEqualIgnoreCase(targetPhrase.OriginPhrase)
            ? QuestionResult.Passed(chat.Texts)
            : QuestionResult.FailedResultPhraseWas(targetPhrase.OriginPhrase, chat.Texts);
    }
}