using System;
using System.Collections.Generic;
using System.Linq;
using Chotiskazal.Bot.ConcreteQuestions;
using SayWhat.Bll.Services;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.Questions;

public class QuestionSelector
{
    private readonly ExamAndPreferredScore _engChoose = new(
        Question: new EngChooseQuestion(),
        ExpectedScore: 0.6,
        Frequency: 7);

    private readonly ExamAndPreferredScore _ruChoose = new(
        Question: new RuChooseQuestion(),
        ExpectedScore: 0.6,
        Frequency: 7);

    private readonly ExamAndPreferredScore _engTrust = new(
        Question: new EnTrustQuestion(),
        ExpectedScore: 2,
        Frequency: 10);

    private readonly ExamAndPreferredScore _ruTrust = new(
        Question: new RuTrustQuestion(),
        ExpectedScore: 2,
        Frequency: 10);

    private readonly ExamAndPreferredScore _ruTrustSingle = new(
        Question: new RuTrustSingleTranslationQuestion(), 
        ExpectedScore: 2,
        Frequency: 10);

    private readonly ExamAndPreferredScore _engPhraseChoose = new(
        Question: new EngChoosePhraseQuestion(),
        ExpectedScore: 6,
        Frequency: 4);

    private readonly ExamAndPreferredScore _ruPhraseChoose = new(
        Question: new RuChoosePhraseQuestion(),
        ExpectedScore: 6,
        Frequency: 4);

        
    private ExamAndPreferredScore _engEasyWriteMissingLetter =>
        new(
            Question: new EngWriteMissingLettersQuestion(StarredHardness.Easy),
            ExpectedScore: 2.1,
            Frequency: 7);
    private ExamAndPreferredScore _ruEasyWriteMissingLetter =>
        new(
            Question: new RuWriteMissingLettersQuestion(StarredHardness.Easy),
            ExpectedScore: 2.1,
            Frequency: 7);
        
    private ExamAndPreferredScore _engHardWriteMissingLetter =>
        new(
            Question: new EngWriteMissingLettersQuestion(StarredHardness.Hard),
            ExpectedScore: 2.6,
            Frequency: 7);
    private ExamAndPreferredScore _ruHardWriteMissingLetter =>
        new(
            Question: new RuWriteMissingLettersQuestion(StarredHardness.Hard),
            ExpectedScore: 2.6,
            Frequency: 7);

    private readonly ExamAndPreferredScore _engChooseWordInPhrase = new(
        new EngChooseWordInPhraseQuestion(), 4, 20);

    private readonly ExamAndPreferredScore _clearEngChooseWordInPhrase = new(
        new ClearScreenQuestionDecorator(new EngChooseWordInPhraseQuestion()), 2.3, 20);

    private readonly ExamAndPreferredScore _engPhraseSubstitute = new(
        Question: new EngPhraseSubstituteQuestion(),
        ExpectedScore: 4,
        Frequency: 12);

    private readonly ExamAndPreferredScore _ruPhraseSubstitute = new(
        Question: new RuPhraseSubstituteQuestion(),
        ExpectedScore: 4,
        Frequency: 12);

    private readonly ExamAndPreferredScore _assemblePhraseExam = new(
        new AssemblePhraseQuestion(), 2.3, 7);

    private readonly ExamAndPreferredScore _clearEngPhraseSubstitute = new(
        Question: new ClearScreenQuestionDecorator(new EngPhraseSubstituteQuestion()),
        ExpectedScore: 6,
        Frequency: 12);

    private readonly ExamAndPreferredScore _clearRuPhraseSubstitute = new(
        Question: new ClearScreenQuestionDecorator(new RuPhraseSubstituteQuestion()),
        ExpectedScore: 6,
        Frequency: 12);

        
    private ExamAndPreferredScore EngWrite(LocalDictionaryService service) =>
        new(
            Question: new EngWriteQuestion(service),
            ExpectedScore: 2.6,
            Frequency: 14);

    private ExamAndPreferredScore RuWrite(LocalDictionaryService service) => new(
        Question: new RuWriteQuestion(service),
        ExpectedScore: 2.6,
        Frequency: 14);
        
    private readonly ExamAndPreferredScore _transcriptionExam = new(
        Question: new TranscriptionChooseQuestion(),
        ExpectedScore: 1.6,
        Frequency:10);

    private readonly ExamAndPreferredScore _engChooseByTranscriptionExam = new(
        Question: new TranscriptionChooseEngQuestion(),
        ExpectedScore: 2.7,
        Frequency: 7);

    private readonly ExamAndPreferredScore _ruChooseByTranscriptionExam = new(
        Question: new TranscriptionChooseRuQuestion(),
        ExpectedScore: 3.3,
        Frequency: 7);
        
    private readonly ExamAndPreferredScore _isItRightTranslationExam = new(
        Question:new IsItRightTranslationQuestion(), 
        ExpectedScore:1.6,
        Frequency:7);

    private readonly ExamAndPreferredScore _engChooseMultipleTranslationExam = new(
        Question:new EngChooseMultipleTranslationsQuestion(),  
        ExpectedScore:1.6,
        Frequency:10);

    private ExamAndPreferredScore RuWriteSingleTranslationExam(LocalDictionaryService service) => new(
        Question:new RuWriteSingleTarnslationQuestion(service),  
        ExpectedScore:1.6,
        Frequency:10);

    public QuestionSelector(LocalDictionaryService localDictionaryService)
    {
        _simpleExamsList = new[]
        {
            _engChoose,
            _ruChoose,
            _ruPhraseChoose,
            _engPhraseChoose,
            _engChooseWordInPhrase,
            _transcriptionExam,
            _isItRightTranslationExam,
            _engChooseMultipleTranslationExam,
        };
        _intermediateExamsList = new[]
        {
            _engEasyWriteMissingLetter,
            _ruEasyWriteMissingLetter,
            _engHardWriteMissingLetter,
            _ruHardWriteMissingLetter,
            _engChoose,
            _ruChoose,
            _ruPhraseChoose,
            _engPhraseChoose,
            _engPhraseSubstitute,
            _ruPhraseSubstitute,
            _assemblePhraseExam,
            _engTrust,
            _ruTrust,
            _ruTrustSingle,
            _engChooseByTranscriptionExam,
            _ruChooseByTranscriptionExam,
            _transcriptionExam,
            _isItRightTranslationExam,
            _engChooseMultipleTranslationExam,
        };
        _advancedExamsList = new[]
        {
            _engHardWriteMissingLetter,
            _ruHardWriteMissingLetter,
            _engChoose,
            _ruChoose,
            _engPhraseChoose,
            _ruPhraseChoose,
            _engTrust,
            _ruTrust,
            _ruTrustSingle,
            EngWrite(localDictionaryService),
            RuWrite(localDictionaryService),
            RuWriteSingleTranslationExam(localDictionaryService),
            _clearEngPhraseSubstitute,
            _clearRuPhraseSubstitute,
            _engPhraseSubstitute,
            _ruPhraseSubstitute,
            _engChooseWordInPhrase,
            _clearEngChooseWordInPhrase,
            _assemblePhraseExam,
            _isItRightTranslationExam,
            _engChooseMultipleTranslationExam,
        };
    }

    private readonly ExamAndPreferredScore[] _simpleExamsList;
    private readonly ExamAndPreferredScore[] _intermediateExamsList;
    private readonly ExamAndPreferredScore[] _advancedExamsList;

    public HashSet<ExamAndPreferredScore> AllQuestions =>
        _simpleExamsList.Concat(_intermediateExamsList).Concat(_advancedExamsList).ToHashSet();
    public IQuestion GetNextQuestionFor(bool isFirstExam, UserWordModel model)
    {
        if (isFirstExam && model.AbsoluteScore < WordLeaningGlobalSettings.LearningWordMinScore)
            return _simpleExamsList.GetRandomItemOrNull().Question;

        //что это?Как выбираются экзамены. Покапоменяла похожим оразом, но надо разобраться, что за score
        var score = model.AbsoluteScore - (isFirstExam ? WordLeaningGlobalSettings.LearningWordMinScore/2 : 0);

        return ChooseExam(score, model.AbsoluteScore < WordLeaningGlobalSettings.WellDoneWordMinScore 
            ? _intermediateExamsList 
            : _advancedExamsList);
    }

    private static IQuestion ChooseExam(double score, ExamAndPreferredScore[] exams)
    {
        score = Math.Min(score, 4.5);
        var probability = new Dictionary<double, IQuestion>(exams.Length);
        double accumulator = 0;
        foreach (var e in exams)
        {
            var delta = e.Frequency / (Math.Abs(e.ExpectedScore - score) + 0.3);
            accumulator += delta;
            probability.Add(accumulator, e.Question);
        }

        var randomValue = Rand.NextDouble() * accumulator;
        var choice = (probability.FirstOrDefault(p => p.Key >= randomValue).Value);
        return choice ?? probability.Last().Value;
    }

    public record ExamAndPreferredScore(IQuestion Question, double ExpectedScore, int Frequency);
}