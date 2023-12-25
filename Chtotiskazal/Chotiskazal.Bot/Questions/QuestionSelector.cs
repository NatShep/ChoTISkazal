using System;
using System.Collections.Generic;
using System.Linq;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.Questions;

public class QuestionSelector {
    private static Question[] CreateBalancedSet(ExamType inputExamType, Question[] subset) {
        var filtered =
            (inputExamType switch
            {
                ExamType.EnInputOnly => subset.Where(s => s.InputType != QuestionInputType.NeedsRuInput),
                ExamType.RuInputOnly => subset.Where(s => s.InputType != QuestionInputType.NeedsEnInput),
                ExamType.NoInput => subset.Where(s => s.InputType == QuestionInputType.NeedsNoInput),
                _ => subset,
            }).ToArray();

        if (inputExamType == ExamType.Everything || inputExamType == ExamType.NoInput)
            return filtered;

        // Отношение частоты письменных экзаменов и кнопочных должно остаться таким же,
        // как и было в оригинальном сете. Для этого посчитаем суммарную частоту писменных экзаменов
        // в оригинальном наборе
        var originWriteFrequencySum =
            subset.Where(s => s.InputType != QuestionInputType.NeedsNoInput).Sum(q => q.Frequency);
        if (originWriteFrequencySum == 0)
            return filtered;

        // Для того что бы относительная частота письменных и кнопочных экзаменов сохранилась,
        // нужно что бы суммарная частота письменных вопросов осталась той же
        // Для этого нужно посчитать на сколько нужно умножить частоту письменных экзаменов
        var filteredWriteFreqSum =
            filtered.Where(s => s.InputType != QuestionInputType.NeedsNoInput).Sum(q => q.Frequency);

        // Разделим originWriteFrequencySum на filteredWriteFreqSum 
        // что бы получить - во сколько раз нужно чаще задавать письменные вопросы
        var writeQuestionsFrequencyModificator = originWriteFrequencySum / (double)filteredWriteFreqSum;

        // Теперь мы можем сформировать сет письменных вопросов с увеличенной частотой
        var writeQuestions = filtered.Where(s => s.InputType != QuestionInputType.NeedsNoInput)
            .Select(q =>
                q.WithFrequency((int)Math.Round(q.Frequency * writeQuestionsFrequencyModificator)));
        //Возьмем кнопочные вопросы и письменные с обновленной частотой, как результат
        return filtered
            .Where(s => s.InputType == QuestionInputType.NeedsNoInput)
            .Concat(writeQuestions)
            .ToArray();
    }

    public QuestionSelector(LocalDictionaryService localDictionaryService) {
        var beginnerList = Questions.BeginnerQuestions;
        var intermediateList = Questions.IntermediateQuestions;
        var advancedList = Questions.AdvancedQuestions(localDictionaryService);
        AllQuestions = beginnerList.Concat(intermediateList).Concat(advancedList).ToHashSet();

        _allQuestionsSet = new ExamQuestionsSet(beginnerList, intermediateList, advancedList);

        _noInputQuestionsSet =
            new ExamQuestionsSet(
                CreateBalancedSet(ExamType.NoInput, beginnerList),
                CreateBalancedSet(ExamType.NoInput, intermediateList),
                CreateBalancedSet(ExamType.NoInput, advancedList));

        _enInputQuestionsSet =
            new ExamQuestionsSet(
                CreateBalancedSet(ExamType.EnInputOnly, beginnerList),
                CreateBalancedSet(ExamType.EnInputOnly, intermediateList),
                CreateBalancedSet(ExamType.EnInputOnly, advancedList));

        _ruInputQuestionsSet =
            new ExamQuestionsSet(
                CreateBalancedSet(ExamType.RuInputOnly, beginnerList),
                CreateBalancedSet(ExamType.RuInputOnly, intermediateList),
                CreateBalancedSet(ExamType.RuInputOnly, advancedList));
    }

    private readonly ExamQuestionsSet _allQuestionsSet;
    private readonly ExamQuestionsSet _ruInputQuestionsSet;
    private readonly ExamQuestionsSet _enInputQuestionsSet;
    private readonly ExamQuestionsSet _noInputQuestionsSet;

    public HashSet<Question> AllQuestions { get; }

    public Question GetNextQuestionFor(UserWordModel model, ExamType examType) {
        var questionSet =
            examType switch
            {
                ExamType.RuInputOnly => _ruInputQuestionsSet,
                ExamType.EnInputOnly => _enInputQuestionsSet,
                ExamType.NoInput => _noInputQuestionsSet,
                ExamType.Everything => _allQuestionsSet,
                _ => throw new ArgumentOutOfRangeException(nameof(examType), examType, null)
            };
        return questionSet.GetNextQuestionFor(model);
    }
}

public enum ExamType {
    RuInputOnly,
    EnInputOnly,
    NoInput,
    Everything
}