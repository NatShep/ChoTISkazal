using System;
using System.Collections.Generic;
using System.Linq;
using SayWhat.Bll;
using SayWhat.Bll.Dto;

namespace Chotiskazal.Bot.Questions
{
    public static class ExamSelector
    {
        private static readonly ExamAndPreferredScore EngChoose = new ExamAndPreferredScore(
            exam: new EngChooseExam(),
            expectedScore: 2,
            frequency: 7);
        private static readonly ExamAndPreferredScore RuChoose = new ExamAndPreferredScore(
            exam: new RuChooseExam(),
            expectedScore: 2,
            frequency: 7);
        private static readonly ExamAndPreferredScore EngTrust = new ExamAndPreferredScore(
            exam: new EnTrustExam(),
            expectedScore: 6,
            frequency: 10);
        private static readonly ExamAndPreferredScore RuTrust = new ExamAndPreferredScore(
            exam: new RuTrustExam(),
            expectedScore: 6,
            frequency: 10);
        private static readonly ExamAndPreferredScore EngPhraseChoose = new ExamAndPreferredScore(
            exam: new EngChoosePhraseExam(),
            expectedScore: 4,
            frequency: 4);
        private static readonly ExamAndPreferredScore RuPhraseChoose = new ExamAndPreferredScore(
            exam: new RuChoosePhraseExam(),
            expectedScore: 4,
            frequency: 4);
        
        private static readonly ExamAndPreferredScore EngChooseWordInPhrase = new ExamAndPreferredScore(
            new EngChooseWordInPhraseExam(),6,20);

        private static readonly ExamAndPreferredScore ClearEngChooseWordInPhrase = new ExamAndPreferredScore(
            new ClearScreenExamDecorator(new EngChooseWordInPhraseExam()), 7, 20);

        private static readonly ExamAndPreferredScore EngPhraseSubstitute = new ExamAndPreferredScore(
            exam: new EngPhraseSubstituteExam(),
            expectedScore: 6,
            frequency: 12);
        private static readonly ExamAndPreferredScore RuPhraseSubstitute = new ExamAndPreferredScore(
            exam: new RuPhraseSubstituteExam(),
            expectedScore: 6,
            frequency: 12);

        private static readonly ExamAndPreferredScore AssemblePhraseExam = new ExamAndPreferredScore(
            new AssemblePhraseExam(), 7, 7);

        private static readonly ExamAndPreferredScore ClearEngPhraseSubstitute = new ExamAndPreferredScore(
            exam: new ClearScreenExamDecorator(new EngPhraseSubstituteExam()), 
            expectedScore: 8,
            frequency: 12);

        private static readonly ExamAndPreferredScore ClearRuPhraseSubstitute = new ExamAndPreferredScore(
            exam: new ClearScreenExamDecorator(new RuPhraseSubstituteExam()),
            expectedScore: 8,
            frequency: 12);

        private static readonly ExamAndPreferredScore EngWrite =
            new ExamAndPreferredScore(
                exam: new EngWriteExam(),
                expectedScore: 8,
                frequency: 14);
       
        private static readonly ExamAndPreferredScore RuWrite = new ExamAndPreferredScore(
            exam: new RuWriteExam(),
            expectedScore: 8,
            frequency: 14);
        
       private static readonly ExamAndPreferredScore HideousEngPhraseChoose = new ExamAndPreferredScore(
            exam: new ClearScreenExamDecorator(new EngChoosePhraseExam()),
            expectedScore: 7,
            frequency: 10);
       
        private static readonly ExamAndPreferredScore HideousRuPhraseChoose = new ExamAndPreferredScore(
            exam: new ClearScreenExamDecorator(new RuChoosePhraseExam()),
            expectedScore: 7,
            frequency: 10);
        
        private static readonly ExamAndPreferredScore HideousEngTrust = new ExamAndPreferredScore(
            exam: new ClearScreenExamDecorator(new EnTrustExam()),
            expectedScore: 10,
            frequency: 2);
        
        private static readonly ExamAndPreferredScore HideousRuTrust =
            new ExamAndPreferredScore(
                exam: new ClearScreenExamDecorator(new RuTrustExam()),
                expectedScore: 10,
                frequency: 3);
        
        private static readonly ExamAndPreferredScore HideousEngWriteExam =
            new ExamAndPreferredScore(
                exam: new ClearScreenExamDecorator(new EngWriteExam()),
                expectedScore: 12,
                frequency: 14);
        
        private static readonly ExamAndPreferredScore HideousRuWriteExam =
            new ExamAndPreferredScore(
                exam: new ClearScreenExamDecorator(new RuWriteExam()),
                expectedScore: 12,
                frequency: 14);

        public static IExam GetNextExamFor(bool isFirstExam, UserWordModel model)
        {
            if (isFirstExam && model.PassedScore < 7)
            {
                var list = new[]
                {
                    EngChoose.Exam,
                    RuChoose.Exam,
                    RuPhraseChoose.Exam,
                    EngPhraseChoose.Exam,  
                    EngChooseWordInPhrase.Exam,
                };
                return list.GetRandomItem();
            }

            var score = model.PassedScore - (isFirstExam ? 2 : 0);

            if (model.PassedScore < 4)
            {
                return ChooseExam(score, new []
                {
                    EngChoose,
                    RuChoose,
                    EngTrust, 
                    RuTrust, 
                    HideousRuTrust, 
                    HideousEngTrust,
                });
            }

            return ChooseExam(score, new []
            {
                EngChoose,
                RuChoose,
                EngPhraseChoose,
                RuPhraseChoose, 
                EngTrust, 
                RuTrust, 
                EngWrite, 
                RuWrite, 
                HideousRuPhraseChoose,
                HideousEngPhraseChoose, 
                HideousEngTrust, 
                HideousRuTrust, 
                HideousEngWriteExam, 
                HideousRuWriteExam,
                ClearEngPhraseSubstitute,
                ClearRuPhraseSubstitute,
                EngPhraseSubstitute,
                RuPhraseSubstitute,
                EngChooseWordInPhrase,
                ClearEngChooseWordInPhrase,
                AssemblePhraseExam,
            });
        }

        private static IExam ChooseExam(int score, ExamAndPreferredScore[] exams)
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

            var randomValue = RandomTools.Rnd.NextDouble() * accumulator;
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
