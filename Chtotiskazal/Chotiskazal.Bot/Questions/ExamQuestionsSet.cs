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
    
    public Question GetNextQuestionFor(bool isFirstExam, UserWordModel model) {
        if (isFirstExam && model.AbsoluteScore < WordLeaningGlobalSettings.LearningWordMinScore)
            return Beginner.GetRandomItemOrNull();

        //что это?Как выбираются экзамены. Покапоменяла похожим оразом, но надо разобраться, что за score
        var score = model.AbsoluteScore - (isFirstExam ? WordLeaningGlobalSettings.LearningWordMinScore / 2 : 0);

        return ChooseQuestion(score, model.AbsoluteScore < WordLeaningGlobalSettings.WellDoneWordMinScore
            ? Intermediate
            : Advanced);
    }

    private static Question ChooseQuestion(double score, Question[] exams) {
        score = Math.Min(score, 4.5);
        var probability = new Dictionary<double, Question>(exams.Length);
        double accumulator = 0;
        foreach (var e in exams) {
            var delta = e.Frequency / (Math.Abs(e.ExpectedScore - score) + 0.3);
            accumulator += delta;
            probability.Add(accumulator, e);
        }

        var randomValue = Rand.NextDouble() * accumulator;
        var choice = (probability.FirstOrDefault(p => p.Key >= randomValue).Value);
        return choice ?? probability.Last().Value;
    }
}