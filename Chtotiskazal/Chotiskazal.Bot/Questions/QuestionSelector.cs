using System;
using System.Collections.Generic;
using System.Linq;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.Questions;

public class QuestionSelector {
    public QuestionSelector(LocalDictionaryService localDictionaryService) {
        _simpleExamsList = Questions.SimpleExamsList;
        _intermediateExamsList = Questions.IntermediateExamsList;
        _advancedExamsList = Questions.AdvancedExamsList(localDictionaryService);
    }

    private readonly Question[] _simpleExamsList;
    private readonly Question[] _intermediateExamsList;
    private readonly Question[] _advancedExamsList;

    public HashSet<Question> AllQuestions =>
        _simpleExamsList.Concat(_intermediateExamsList).Concat(_advancedExamsList).ToHashSet();

    public Question GetNextQuestionFor(bool isFirstExam, UserWordModel model) {
        if (isFirstExam && model.AbsoluteScore < WordLeaningGlobalSettings.LearningWordMinScore)
            return _simpleExamsList.GetRandomItemOrNull();

        //что это?Как выбираются экзамены. Покапоменяла похожим оразом, но надо разобраться, что за score
        var score = model.AbsoluteScore - (isFirstExam ? WordLeaningGlobalSettings.LearningWordMinScore / 2 : 0);

        return ChooseQuestion(score, model.AbsoluteScore < WordLeaningGlobalSettings.WellDoneWordMinScore
            ? _intermediateExamsList
            : _advancedExamsList);
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