using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dic.Logic;
using Dic.Logic.DAL;

namespace Dic.AddWords.ConsoleApp.Exams
{
    public static class ExamHelper
    {
        static ExamHelper()
        {
            Exams = new[]
            {
                new ExamAndPreferedScore(
                    exam: new EngChooseExam(), 
                    expectedScore: 0, 
                    frequency:     10),
                new ExamAndPreferedScore(
                    exam: new RuChooseExam(), 
                    expectedScore: 0, 
                    frequency:     10),
                new ExamAndPreferedScore(
                    exam: new EnTrustExam(), 
                    expectedScore: 4, 
                    frequency:     10),
                new ExamAndPreferedScore(
                    exam: new RuTrustExam(), 
                    expectedScore: 4,
                    frequency:     10),
                new ExamAndPreferedScore(
                    exam: new EngWriteExam(), 
                    expectedScore: 6,
                    frequency:     10),
                new ExamAndPreferedScore(
                    exam: new RuWriteExam(), 
                    expectedScore: 6,
                    frequency    : 10),
              new ExamAndPreferedScore(
                    exam: new CleatScreenExamDecorator(new EnTrustExam()), 
                    expectedScore: 6,
                    frequency:     2),
                new ExamAndPreferedScore(
                    exam: new CleatScreenExamDecorator(new RuTrustExam()), 
                    expectedScore: 6,
                    frequency:     3),
                new ExamAndPreferedScore(
                    exam: new CleatScreenExamDecorator(new EngWriteExam()), 
                    expectedScore: 8,
                    frequency:     10),
                new ExamAndPreferedScore(
                    exam: new CleatScreenExamDecorator(new RuWriteExam()), 
                    expectedScore: 8,
                    frequency:     10)
            };
        }

        private static ExamAndPreferedScore[] Exams { get; }
        public static IExam GetNextExamFor(bool isFirstExam, PairModel model)
        {
            int score = model.PassedScore - (isFirstExam ? 5 : 0);
            Dictionary<double, IExam> probability = new Dictionary<double, IExam>(Exams.Length);
            double accumulator = 0;
            foreach (var e in Exams)
            {
                var delta = e.Frequency / (Math.Abs(e.ExpectedScore - score) +1.0);
                accumulator += delta;
                probability.Add(accumulator, e.Exam);
            }

            var randomValue =  RandomTools.Rnd.NextDouble() * accumulator;
            var choice = (probability.FirstOrDefault(p => p.Key >= randomValue).Value) ;
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
