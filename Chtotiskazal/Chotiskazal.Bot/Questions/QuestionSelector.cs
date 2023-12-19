using System;
using System.Collections.Generic;
using System.Linq;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.Questions;

public class QuestionSelector {
    public QuestionSelector(LocalDictionaryService localDictionaryService) {
        var beginnerList = Questions.BeginnerQuestions;
        var intermediateList = Questions.IntermediateQuestions;
        var advancedList = Questions.AdvancedQuestions(localDictionaryService);
        
        AllQuestions = beginnerList.Concat(intermediateList).Concat(advancedList).ToHashSet();
        
        _allQuestionsSet = new ExamQuestionsSet(beginnerList, intermediateList, advancedList);
        
        _noInputQuestionsSet = new ExamQuestionsSet(
            beginnerList.Where(s => s.InputType == QuestionInputType.NeedsNoInput).ToArray(),
            intermediateList.Where(s => s.InputType == QuestionInputType.NeedsNoInput).ToArray(),
            advancedList.Where(s => s.InputType == QuestionInputType.NeedsNoInput).ToArray());
        
        _ruInputQuestionsSet = new ExamQuestionsSet(
            beginnerList.Where(s => s.InputType != QuestionInputType.NeedsEnInput).ToArray(),
            intermediateList.Where(s => s.InputType != QuestionInputType.NeedsEnInput).ToArray(),
            advancedList.Where(s => s.InputType != QuestionInputType.NeedsEnInput).ToArray());
        
        _enInputQuestionsSet = new ExamQuestionsSet(
            beginnerList.Where(s => s.InputType != QuestionInputType.NeedsRuInput).ToArray(),
            intermediateList.Where(s => s.InputType != QuestionInputType.NeedsRuInput).ToArray(),
            advancedList.Where(s => s.InputType != QuestionInputType.NeedsRuInput).ToArray());
    }

    private readonly ExamQuestionsSet _allQuestionsSet;
    private readonly ExamQuestionsSet _ruInputQuestionsSet;
    private readonly ExamQuestionsSet _enInputQuestionsSet;
    private readonly ExamQuestionsSet _noInputQuestionsSet;


    public HashSet<Question> AllQuestions { get; }

    public Question GetNextQuestionFor(bool isFirstExam, UserWordModel model, ExamType examType) {
        var questionSet =
            examType switch
            {
                ExamType.RuInputOnly => _ruInputQuestionsSet,
                ExamType.EnInputOnly => _enInputQuestionsSet,
                ExamType.NoInput => _noInputQuestionsSet,
                ExamType.Everything => _allQuestionsSet,
                _ => throw new ArgumentOutOfRangeException(nameof(examType), examType, null)
            };
        if (isFirstExam && model.AbsoluteScore < WordLeaningGlobalSettings.LearningWordMinScore)
            return questionSet.Beginner.GetRandomItemOrNull();

        //что это?Как выбираются экзамены. Покапоменяла похожим оразом, но надо разобраться, что за score
        var score = model.AbsoluteScore - (isFirstExam ? WordLeaningGlobalSettings.LearningWordMinScore / 2 : 0);

        return ChooseQuestion(score, model.AbsoluteScore < WordLeaningGlobalSettings.WellDoneWordMinScore
            ? questionSet.Intermediate
            : questionSet.Advanced);
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

public record ExamQuestionsSet(Question[] Beginner, Question[] Intermediate, Question[] Advanced);

public enum ExamType {
    RuInputOnly,
    EnInputOnly,
    NoInput,
    Everything
}