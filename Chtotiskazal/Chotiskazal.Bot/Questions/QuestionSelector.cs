using System;
using System.Collections.Generic;
using System.Linq;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.Questions
{
    public class QuestionSelector
    {
        public static QuestionSelector Singletone { get; set; }

        private readonly ExamAndPreferredScore _engChoose = new ExamAndPreferredScore(
            exam: new EngChooseExam(),
            expectedScore: 0.6,
            frequency: 7);

        private readonly ExamAndPreferredScore _ruChoose = new ExamAndPreferredScore(
            exam: new RuChooseExam(),
            expectedScore: 0.6,
            frequency: 7);

        private readonly ExamAndPreferredScore _engTrust = new ExamAndPreferredScore(
            exam: new EnTrustExam(),
            expectedScore: 2,
            frequency: 10);

        private readonly ExamAndPreferredScore _ruTrust = new ExamAndPreferredScore(
            exam: new RuTrustExam(),
            expectedScore: 2,
            frequency: 10);

        private readonly ExamAndPreferredScore _engPhraseChoose = new ExamAndPreferredScore(
            exam: new EngChoosePhraseExam(),
            expectedScore: 1.3,
            frequency: 4);

        private readonly ExamAndPreferredScore _ruPhraseChoose = new ExamAndPreferredScore(
            exam: new RuChoosePhraseExam(),
            expectedScore: 1.3,
            frequency: 4);

        private readonly ExamAndPreferredScore _engChooseWordInPhrase = new ExamAndPreferredScore(
            new EngChooseWordInPhraseExam(), 2, 20);

        private readonly ExamAndPreferredScore _clearEngChooseWordInPhrase = new ExamAndPreferredScore(
            new ClearScreenExamDecorator(new EngChooseWordInPhraseExam()), 2.3, 20);

        private readonly ExamAndPreferredScore _engPhraseSubstitute = new ExamAndPreferredScore(
            exam: new EngPhraseSubstituteExam(),
            expectedScore: 2,
            frequency: 12);

        private readonly ExamAndPreferredScore _ruPhraseSubstitute = new ExamAndPreferredScore(
            exam: new RuPhraseSubstituteExam(),
            expectedScore: 2,
            frequency: 12);

        private readonly ExamAndPreferredScore _assemblePhraseExam = new ExamAndPreferredScore(
            new AssemblePhraseExam(), 2.3, 7);

        private readonly ExamAndPreferredScore _clearEngPhraseSubstitute = new ExamAndPreferredScore(
            exam: new ClearScreenExamDecorator(new EngPhraseSubstituteExam()),
            expectedScore: 2.6,
            frequency: 12);

        private readonly ExamAndPreferredScore _clearRuPhraseSubstitute = new ExamAndPreferredScore(
            exam: new ClearScreenExamDecorator(new RuPhraseSubstituteExam()),
            expectedScore: 2.6,
            frequency: 12);

        private ExamAndPreferredScore EngWrite(DictionaryService service) =>
            new ExamAndPreferredScore(
                exam: new EngWriteExam(service),
                expectedScore: 2.6,
                frequency: 14);

        private ExamAndPreferredScore RuWrite(DictionaryService service) => new ExamAndPreferredScore(
            exam: new RuWriteExam(service),
            expectedScore: 2.6,
            frequency: 14);

        private readonly ExamAndPreferredScore _hideousEngPhraseChoose = new ExamAndPreferredScore(
            exam: new ClearScreenExamDecorator(new EngChoosePhraseExam()),
            expectedScore: 2.3,
            frequency: 10);

        private readonly ExamAndPreferredScore _hideousRuPhraseChoose = new ExamAndPreferredScore(
            exam: new ClearScreenExamDecorator(new RuChoosePhraseExam()),
            expectedScore: 2.3,
            frequency: 10);

        private readonly ExamAndPreferredScore _hideousEngTrust = new ExamAndPreferredScore(
            exam: new ClearScreenExamDecorator(new EnTrustExam()),
            expectedScore: 3.3,
            frequency: 2);

        private readonly ExamAndPreferredScore _hideousRuTrust =
            new ExamAndPreferredScore(
                exam: new ClearScreenExamDecorator(new RuTrustExam()),
                expectedScore: 3.3,
                frequency: 3);

        private ExamAndPreferredScore HideousEngWriteExam(DictionaryService service) =>
            new ExamAndPreferredScore(
                exam: new ClearScreenExamDecorator(new EngWriteExam(service)),
                expectedScore: 4,
                frequency: 14);

        private ExamAndPreferredScore HideousRuWriteExam(DictionaryService service) =>
            new ExamAndPreferredScore(
                exam: new ClearScreenExamDecorator(new RuWriteExam(service)),
                expectedScore: 4,
                frequency: 14);

        private readonly ExamAndPreferredScore _transcriptionExam = new ExamAndPreferredScore(
            exam: new TranscriptionChooseExam(),
            expectedScore: 1.6,
            frequency:10);
        
        private readonly ExamAndPreferredScore _hideousTranscriptionExam = new ExamAndPreferredScore(
            exam: new ClearScreenExamDecorator(new TranscriptionChooseExam()),
            expectedScore: 2.6,
            frequency:10);


        private readonly ExamAndPreferredScore _ruChooseByTranscriptionExam = new ExamAndPreferredScore(
            exam: new RuChooseByTranscriptionExam(),
            expectedScore: 3.3,
            frequency: 10);
        
        private readonly ExamAndPreferredScore _hideousRuChooseByTranscriptionExam = new ExamAndPreferredScore(
            exam: new ClearScreenExamDecorator(new RuChooseByTranscriptionExam()),
            expectedScore: 4,
            frequency: 10);
        
        private readonly ExamAndPreferredScore _isItRightTranslationExam = new ExamAndPreferredScore(
            exam:new IsItRightTranslationExam(), 
            expectedScore:1.6,
            frequency:10);
        
        private readonly ExamAndPreferredScore _hideousIsItRightTranslationExam = new ExamAndPreferredScore(
            exam: new ClearScreenExamDecorator(new IsItRightTranslationExam()), 
            expectedScore:2.3,
            frequency:10);
        
        private readonly ExamAndPreferredScore _engChooseMultipleTranslationExam = new ExamAndPreferredScore(
            exam:new EngChooseMultipleTranslationsExam(),  
            expectedScore:1.6,
            frequency:10);

        private readonly ExamAndPreferredScore _hideousEngChooseMultipleTranslationExam = new ExamAndPreferredScore(
            exam: new ClearScreenExamDecorator(new EngChooseMultipleTranslationsExam()),
            expectedScore: 2.3,
            frequency: 10);

        public QuestionSelector(DictionaryService dictionaryService)
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
            _intermidiateExamsList = new[]
            {
                _engChoose,
                _ruChoose,
                _ruPhraseChoose,
                _engPhraseChoose,
                _engTrust,
                _ruTrust,
                _hideousRuTrust,
                _hideousEngTrust,
                _ruChooseByTranscriptionExam,
                _transcriptionExam,
                _isItRightTranslationExam,
                _engChooseMultipleTranslationExam,
                _hideousEngChooseMultipleTranslationExam,
            };
            _advancedExamsList = new[]
            {
                _engChoose,
                _ruChoose,
                _engPhraseChoose,
                _ruPhraseChoose,
                _engTrust,
                _ruTrust,
                EngWrite(dictionaryService),
                RuWrite(dictionaryService),
                _hideousRuPhraseChoose,
                _hideousEngPhraseChoose,
                _hideousEngTrust,
                _hideousRuTrust,
                HideousEngWriteExam(dictionaryService),
                HideousRuWriteExam(dictionaryService),
                _clearEngPhraseSubstitute,
                _clearRuPhraseSubstitute,
                _engPhraseSubstitute,
                _ruPhraseSubstitute,
                _engChooseWordInPhrase,
                _clearEngChooseWordInPhrase,
                _assemblePhraseExam,
               _ruChooseByTranscriptionExam,
               _hideousRuChooseByTranscriptionExam,
               _hideousTranscriptionExam,
               _hideousIsItRightTranslationExam,
                _transcriptionExam,
                _isItRightTranslationExam,
                _engChooseMultipleTranslationExam,
                _hideousEngChooseMultipleTranslationExam,
            };
        }

        private readonly ExamAndPreferredScore[] _simpleExamsList;
        private readonly ExamAndPreferredScore[] _intermidiateExamsList;
        private readonly ExamAndPreferredScore[] _advancedExamsList;

        public IExam GetNextQuestionFor(bool isFirstExam, UserWordModel model)
        {
            if (isFirstExam && model.AbsoluteScore < WordLeaningGlobalSettings.IncompleteWordMinScore)
                return _simpleExamsList.GetRandomItem().Exam;

            var score = model.AbsoluteScore - (isFirstExam ? (WordLeaningGlobalSettings.LearnedWordMinScore/2) : 0);

            if (model.AbsoluteScore < WordLeaningGlobalSettings.FamiliarWordMinScore)
                return ChooseExam(score, _intermidiateExamsList);
            else
                return ChooseExam(score, _advancedExamsList);
        }

        private static IExam ChooseExam(double score, ExamAndPreferredScore[] exams)
        {
            score = Math.Min(score, 4.5);
            var probability = new Dictionary<double, IExam>(exams.Length);
            double accumulator = 0;
            foreach (var e in exams)
            {
                var delta = e.Frequency / (Math.Abs(e.ExpectedScore - score) + 0.3);
                accumulator += delta;
                probability.Add(accumulator, e.Exam);
            }

            var randomValue = Rand.NextDouble() * accumulator;
            var choice = (probability.FirstOrDefault(p => p.Key >= randomValue).Value);
            return choice ?? probability.Last().Value;
        }

        private class ExamAndPreferredScore
        {
            public ExamAndPreferredScore(IExam exam, double expectedScore, int frequency)
            {
                Exam = exam;
                ExpectedScore = expectedScore;
                Frequency = frequency;
            }

            public IExam Exam { get; }
            public double ExpectedScore { get; }
            public int Frequency { get; }
        }
    }
}
