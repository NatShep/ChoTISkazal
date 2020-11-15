using System;
using System.Collections.Generic;
using System.Linq;
using Chotiskazal.Api.Services;
using Chotiskazal.DAL;
using Chotiskazal.DAL.Services;
using Chotiskazal.LogicR;

namespace Chotiskazal.ApI.Exams
{
    public static class ExamSelector
    {
        private static readonly ExamAndPreferedScore EngChoose = new ExamAndPreferedScore(
            exam: new EngChooseExam(),
            expectedScore: 2,
            frequency: 7);
        private static readonly ExamAndPreferedScore RuChoose = new ExamAndPreferedScore(
            exam: new RuChooseExam(),
            expectedScore: 2,
            frequency: 7);
        private static readonly ExamAndPreferedScore EngTrust = new ExamAndPreferedScore(
            exam: new EnTrustExam(),
            expectedScore: 6,
            frequency: 10);
        private static readonly ExamAndPreferedScore RuTrust = new ExamAndPreferedScore(
            exam: new RuTrustExam(),
            expectedScore: 6,
            frequency: 10);
        private static readonly ExamAndPreferedScore EngPhraseChoose = new ExamAndPreferedScore(
            exam: new EngChoosePhraseExam(),
            expectedScore: 4,
            frequency: 4);
        private static readonly ExamAndPreferedScore RuPhraseChoose = new ExamAndPreferedScore(
            exam: new RuChoosePhraseExam(),
            expectedScore: 4,
            frequency: 4);
        
        private static readonly ExamAndPreferedScore EngChooseWordInPhrase = new ExamAndPreferedScore(
            new EngChooseWordInPhraseExam(),6,20);

        private static readonly ExamAndPreferedScore ClearEngChooseWordInPhrase = new ExamAndPreferedScore(
            new ClearScreenExamDecorator(new EngChooseWordInPhraseExam()), 7, 20);

        private static readonly ExamAndPreferedScore EngPhraseSubstitude = new ExamAndPreferedScore(
            exam: new EngPhraseSubstitudeExam(),
            expectedScore: 6,
            frequency: 12);
        private static readonly ExamAndPreferedScore RuPhraseSubstitude = new ExamAndPreferedScore(
            exam: new RuPhraseSubstitudeExam(),
            expectedScore: 6,
            frequency: 12);

        private static readonly ExamAndPreferedScore ClearEngAssemblePhraseExam = new ExamAndPreferedScore(
            new ClearAssemblePhraseExam(), 7, 7);

        private static readonly ExamAndPreferedScore ClearEngPhraseSubstitude = new ExamAndPreferedScore(
            exam: new ClearScreenExamDecorator(new EngPhraseSubstitudeExam()), 
            expectedScore: 8,
            frequency: 12);

        private static readonly ExamAndPreferedScore ClearRuPhraseSubstitude = new ExamAndPreferedScore(
            exam: new ClearScreenExamDecorator(new RuPhraseSubstitudeExam()),
            expectedScore: 8,
            frequency: 12);

        private static readonly ExamAndPreferedScore EngWrite =
            new ExamAndPreferedScore(
                exam: new EngWriteExam(),
                expectedScore: 8,
                frequency: 14);
        /*
        pr8ivate static readonly ExamAndPreferedScore RuWrite = new ExamAndPreferedScore(
            exam: new RuWriteExam(),
            expectedScore: 8,
            frequency: 14);
        */
        
        private static readonly ExamAndPreferedScore HideousEngPhraseChoose = new ExamAndPreferedScore(
            exam: new ClearScreenExamDecorator(new EngChoosePhraseExam()),
            expectedScore: 7,
            frequency: 10);
        private static readonly ExamAndPreferedScore HideousRuPhraseChoose = new ExamAndPreferedScore(
            exam: new ClearScreenExamDecorator(new RuChoosePhraseExam()),
            expectedScore: 7,
            frequency: 10);
        private static readonly ExamAndPreferedScore HideousEngTrust = new ExamAndPreferedScore(
            exam: new ClearScreenExamDecorator(new EnTrustExam()),
            expectedScore: 10,
            frequency: 2);
        private static readonly ExamAndPreferedScore HideousRuTrust =
            new ExamAndPreferedScore(
                exam: new ClearScreenExamDecorator(new RuTrustExam()),
                expectedScore: 10,
                frequency: 3);
        private static readonly ExamAndPreferedScore HideousEngWriteExam =
            new ExamAndPreferedScore(
                exam: new ClearScreenExamDecorator(new EngWriteExam()),
                expectedScore: 12,
                frequency: 14);
       
        /*private static readonly ExamAndPreferedScore HideousRuWriteExam =
            new ExamAndPreferedScore(
                exam: new ClearScreenExamDecorator(new RuWriteExam()),
                expectedScore: 12,
                frequency: 14);
*/
        public static IExam GetNextExamFor(bool isFirstExam, UserWordForLearning model)
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

            int score = model.PassedScore - (isFirstExam ? 2 : 0);

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
       //         RuWrite, 
                HideousRuPhraseChoose,
                HideousEngPhraseChoose,
                HideousEngTrust, 
                HideousRuTrust, 
                HideousEngWriteExam, 
         //       HideousRuWriteExam,
                ClearEngPhraseSubstitude,
                ClearRuPhraseSubstitude,
                EngPhraseSubstitude,
                RuPhraseSubstitude,
                EngChooseWordInPhrase,
                ClearEngChooseWordInPhrase,
                ClearEngAssemblePhraseExam,
            });
        }

        private static IExam ChooseExam(int score, ExamAndPreferedScore[] exams)
        {
            score = Math.Min(score, 14);
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
