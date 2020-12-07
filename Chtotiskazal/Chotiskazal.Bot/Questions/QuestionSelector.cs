using System;
using System.Collections.Generic;
using System.Linq;
using SayWhat.Bll;
using SayWhat.Bll.Services;
using Random = SayWhat.Bll.Random;

namespace Chotiskazal.Bot.Questions
{
    public class QuestionSelector
    {
        public static QuestionSelector Singletone { get; set; }

        private readonly ExamAndPreferredScore _engChoose = new ExamAndPreferredScore(
            exam: new EngChooseExam(),
            expectedScore: 2,
            frequency: 7);

        private readonly ExamAndPreferredScore _ruChoose = new ExamAndPreferredScore(
            exam: new RuChooseExam(),
            expectedScore: 2,
            frequency: 7);

        private readonly ExamAndPreferredScore _engTrust = new ExamAndPreferredScore(
            exam: new EnTrustExam(),
            expectedScore: 6,
            frequency: 10);

        private readonly ExamAndPreferredScore _ruTrust = new ExamAndPreferredScore(
            exam: new RuTrustExam(),
            expectedScore: 6,
            frequency: 10);

        private readonly ExamAndPreferredScore _engPhraseChoose = new ExamAndPreferredScore(
            exam: new EngChoosePhraseExam(),
            expectedScore: 4,
            frequency: 4);

        private readonly ExamAndPreferredScore _ruPhraseChoose = new ExamAndPreferredScore(
            exam: new RuChoosePhraseExam(),
            expectedScore: 4,
            frequency: 4);

        private readonly ExamAndPreferredScore _engChooseWordInPhrase = new ExamAndPreferredScore(
            new EngChooseWordInPhraseExam(), 6, 20);

        private readonly ExamAndPreferredScore _clearEngChooseWordInPhrase = new ExamAndPreferredScore(
            new ClearScreenExamDecorator(new EngChooseWordInPhraseExam()), 7, 20);

        private readonly ExamAndPreferredScore _engPhraseSubstitute = new ExamAndPreferredScore(
            exam: new EngPhraseSubstituteExam(),
            expectedScore: 6,
            frequency: 12);

        private readonly ExamAndPreferredScore _ruPhraseSubstitute = new ExamAndPreferredScore(
            exam: new RuPhraseSubstituteExam(),
            expectedScore: 6,
            frequency: 12);

        private readonly ExamAndPreferredScore _assemblePhraseExam = new ExamAndPreferredScore(
            new AssemblePhraseExam(), 7, 7);

        private readonly ExamAndPreferredScore _clearEngPhraseSubstitute = new ExamAndPreferredScore(
            exam: new ClearScreenExamDecorator(new EngPhraseSubstituteExam()),
            expectedScore: 8,
            frequency: 12);

        private readonly ExamAndPreferredScore _clearRuPhraseSubstitute = new ExamAndPreferredScore(
            exam: new ClearScreenExamDecorator(new RuPhraseSubstituteExam()),
            expectedScore: 8,
            frequency: 12);

        private ExamAndPreferredScore EngWrite(DictionaryService service) =>
            new ExamAndPreferredScore(
                exam: new EngWriteExam(service),
                expectedScore: 8,
                frequency: 14);

        private ExamAndPreferredScore RuWrite(DictionaryService service) => new ExamAndPreferredScore(
            exam: new RuWriteExam(service),
            expectedScore: 8,
            frequency: 14);

        private readonly ExamAndPreferredScore _hideousEngPhraseChoose = new ExamAndPreferredScore(
            exam: new ClearScreenExamDecorator(new EngChoosePhraseExam()),
            expectedScore: 7,
            frequency: 10);

        private readonly ExamAndPreferredScore _hideousRuPhraseChoose = new ExamAndPreferredScore(
            exam: new ClearScreenExamDecorator(new RuChoosePhraseExam()),
            expectedScore: 7,
            frequency: 10);

        private readonly ExamAndPreferredScore _hideousEngTrust = new ExamAndPreferredScore(
            exam: new ClearScreenExamDecorator(new EnTrustExam()),
            expectedScore: 10,
            frequency: 2);

        private readonly ExamAndPreferredScore _hideousRuTrust =
            new ExamAndPreferredScore(
                exam: new ClearScreenExamDecorator(new RuTrustExam()),
                expectedScore: 10,
                frequency: 3);

        private ExamAndPreferredScore HideousEngWriteExam(DictionaryService service) =>
            new ExamAndPreferredScore(
                exam: new ClearScreenExamDecorator(new EngWriteExam(service)),
                expectedScore: 12,
                frequency: 14);

        private ExamAndPreferredScore HideousRuWriteExam(DictionaryService service) =>
            new ExamAndPreferredScore(
                exam: new ClearScreenExamDecorator(new RuWriteExam(service)),
                expectedScore: 12,
                frequency: 14);

        private readonly ExamAndPreferredScore _transcriptionExam = new ExamAndPreferredScore(
            exam: new TranscriptionChooseExam(),
            expectedScore: 5,
            frequency:10);


        private readonly ExamAndPreferredScore _RuChooseByTranscriptionExam = new ExamAndPreferredScore(
            exam: new RuChooseByTranscriptionExam(),
            expectedScore: 10,
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
             //
                _RuChooseByTranscriptionExam,
                _transcriptionExam
            };
            _intermidiateExamsList = new[]
            {
                _engChoose,
                _ruChoose,
                _engTrust,
                _ruTrust,
                _hideousRuTrust,
                _hideousEngTrust,
                //
                _RuChooseByTranscriptionExam,
                _transcriptionExam
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
                //
                _RuChooseByTranscriptionExam,
                _transcriptionExam
            };
        }

        private readonly ExamAndPreferredScore[] _simpleExamsList;
        private readonly ExamAndPreferredScore[] _intermidiateExamsList;
        private readonly ExamAndPreferredScore[] _advancedExamsList;

        public IExam GetNextQuestionFor(bool isFirstExam, UserWordModel model)
        {
            if (isFirstExam && model.AbsoluteScore < 7)
                return _simpleExamsList.GetRandomItem().Exam;

            var score = model.AbsoluteScore - (isFirstExam ? 2 : 0);

            if (model.AbsoluteScore < 4)
                return ChooseExam(score, _intermidiateExamsList);
            else
                return ChooseExam(score, _advancedExamsList);
        }

        private static IExam ChooseExam(double score, ExamAndPreferredScore[] exams)
        {
            score = Math.Min(score, 14);
            var probability = new Dictionary<double, IExam>(exams.Length);
            double accumulator = 0;
            foreach (var e in exams)
            {
                var delta = e.Frequency / (Math.Abs(e.ExpectedScore - score) + 1.0);
                accumulator += delta;
                probability.Add(accumulator, e.Exam);
            }

            var randomValue = Random.Rnd.NextDouble() * accumulator;
            var choice = (probability.FirstOrDefault(p => p.Key >= randomValue).Value);
            return choice ?? probability.Last().Value;
        }

        private class ExamAndPreferredScore
        {
            public ExamAndPreferredScore(IExam exam, int expectedScore, int frequency)
            {
                Exam = exam;
                ExpectedScore = expectedScore;
                Frequency = frequency;
            }

            public IExam Exam { get; }
            public int ExpectedScore { get; }
            public int Frequency { get; }
        }
    }
}
