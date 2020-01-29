using System;
using System.Collections.Generic;
using System.Linq;
using Dic.Logic;
using Dic.Logic.DAL;

namespace Dic.AddWords.ConsoleApp.Exams
{
    public static class ExamHelper
    {
        private static readonly ExamAndPreferedScore EngPhraseChoose = new ExamAndPreferedScore(
            exam: new EngChoosePhraseExam(), 
            expectedScore: 4,
            frequency: 20);
        private static readonly ExamAndPreferedScore EngChoose = new ExamAndPreferedScore(
            exam: new EngChooseExam(),
            expectedScore: 4,
            frequency: 10);

        private static readonly ExamAndPreferedScore RuChoose = new ExamAndPreferedScore(
            exam: new RuChooseExam(),
            expectedScore: 4,
            frequency: 10);

        private static readonly ExamAndPreferedScore EngTrust = new ExamAndPreferedScore(
            exam: new EnTrustExam(),
            expectedScore: 6,
            frequency: 10);

        private static readonly ExamAndPreferedScore RuTrust = new ExamAndPreferedScore(
            exam: new RuTrustExam(),
            expectedScore: 6,
            frequency: 10);

        private static readonly ExamAndPreferedScore EngWrite =
            new ExamAndPreferedScore(
                exam: new EngWriteExam(),
                expectedScore: 8,
                frequency: 10);

        private static readonly ExamAndPreferedScore RuWrite = new ExamAndPreferedScore(
            exam: new RuWriteExam(),
            expectedScore: 8,
            frequency: 10);

        private static readonly ExamAndPreferedScore HideousEngTrust = new ExamAndPreferedScore(
            exam: new CleatScreenExamDecorator(new EnTrustExam()),
            expectedScore: 10,
            frequency: 2);

        private static readonly ExamAndPreferedScore HideousRuTrust =
            new ExamAndPreferedScore(
                exam: new CleatScreenExamDecorator(new RuTrustExam()),
                expectedScore: 10,
                frequency: 3);

        private static readonly ExamAndPreferedScore HideousEngWriteExam =
            new ExamAndPreferedScore(
                exam: new CleatScreenExamDecorator(new EngWriteExam()),
                expectedScore: 12,
                frequency: 10);

        private static readonly ExamAndPreferedScore HideousRuWriteExam =
            new ExamAndPreferedScore(
                exam: new CleatScreenExamDecorator(new RuWriteExam()),
                expectedScore: 12,
                frequency: 10);


        public static IExam GetNextExamFor(bool isFirstExam, PairModel model)
        {
            if (isFirstExam && model.PassedScore < 8)
            {
                var list = new IExam[]
                {
                    new EngChooseExam(),
                    new RuChooseExam(),
                };
                return list.GetRandomItem();
            }

            int score = model.PassedScore - (isFirstExam ? 2 : 0);

            if (model.PassedScore < 4)
            {
                return ChooseExam(score, new []
                {
                    EngChoose,RuChoose,EngPhraseChoose, EngTrust, RuTrust, HideousRuTrust, HideousEngTrust
                });
            }

            return ChooseExam(score, new []
            {
                EngChoose,RuChoose,EngPhraseChoose, EngTrust, RuTrust, EngWrite, RuWrite, HideousEngTrust, HideousRuTrust, HideousEngWriteExam, HideousRuWriteExam
            });
        }

        private static IExam ChooseExam(int score, ExamAndPreferedScore[] exams)
        {
            Dictionary<double, IExam> probability = new Dictionary<double, IExam>(exams.Length);
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
        class ExamAndPreferedScore
        {
            public ExamAndPreferedScore(IExam exam, int expectedScore, int frequency)
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
