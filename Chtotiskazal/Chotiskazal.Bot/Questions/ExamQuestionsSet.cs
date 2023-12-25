using System;
using System.Collections.Generic;
using System.Linq;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.Questions;

public class ExamQuestionsSet {
    public ExamQuestionsSet(Question[] beginner, Question[] intermediate, Question[] advanced) {
        Beginner = beginner;
        Intermediate = intermediate;
        Advanced = advanced;
    }

    private Question[] Beginner { get; }
    private Question[] Intermediate { get; }
    private Question[] Advanced { get; }
    
    public Question GetNextQuestionFor(UserWordModel model) {
        if (model.AbsoluteScore < WordLeaningGlobalSettings.LearningWordMinScore)
            return PhraseOrWordQuestions(Beginner, model.IsWord).GetRandomItemOrNull();

        //что это?Как выбираются экзамены. Покапоменяла похожим оразом, но надо разобраться, что за score
        return ChooseQuestion(model.AbsoluteScore, model.AbsoluteScore < WordLeaningGlobalSettings.WellDoneWordMinScore
            ? PhraseOrWordQuestions(Intermediate, model.IsWord)
            : PhraseOrWordQuestions(Advanced, model.IsWord));
    }

    private static Question ChooseQuestion(double score, Question[] exams) {
        score = Math.Min(score, WordLeaningGlobalSettings.WellDoneWordMinScore);
        var probability = new Dictionary<double, Question>(exams.Length);
        double accumulator = 0;
        foreach (var e in exams) {
            var delta = e.Frequency / (Math.Abs(e.ExpectedScore - score) + 1);
            accumulator += delta;
            probability.Add(accumulator, e);
        }

        var randomValue = Rand.NextDouble() * accumulator;
        var choice = (probability.FirstOrDefault(p => p.Key >= randomValue).Value);
        return choice ?? probability.Last().Value;
    }

    private static Question[] PhraseOrWordQuestions(Question[] questions, bool isWord) {
        if (isWord)
            return questions.Where(q => q.Fit != ScenarioWordTypeFit.OnlyPhrase).ToArray();
        else
            return questions.Where(q => q.Fit != ScenarioWordTypeFit.OnlyWord).ToArray();
    }
}