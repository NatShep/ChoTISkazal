using System;
using Chotiskazal.LogicR;

namespace Chotiskazal.Dal
{
    public class QuestionMetric
    {
        public const int MaxExamScore = 10;
        public const int PenaltyScore = 9;

        private const int ExamFailedPenalty = 2;
        private const double AgingFactor = 1;
        public const double ReducingPerPointFactor = 1.7;
        //res reduces for 1 point per AgingFactor days
        public double AggedScore => Math.Max(0, PassedScore - (DateTime.Now - LastExam).TotalDays / AgingFactor);


        public QuestionMetric()
        {
            ElaspedMs = 0;
            Result = 0;
            Type = "";
            Revision = 0;
            PreviousExam=DateTime.Now;
            LastExam=DateTime.Now;
            ExamsPassed = 0;
            Examed = 0;
            PassedScore = 0;
            AggregateScore = 0;
            AggregateScoreBefore = 0;
            PassedScoreBefore = 0;
        }

        public int Id { get; set; }
        public int ElaspedMs { get; set; }
        public double AggregateScore { get; set; }
        public double AggregateScoreBefore { get; set; }
        public double PassedScoreBefore { get; set; }
        public int Result { get; set; }
        public string Type { get; set; }

        public DateTime PreviousExam { get; set; }
        public int ExamsPassed { get; set; }

        public DateTime LastExam { get; set; }
        public int PassedScore { get; set; }

        public int Examed { get; set; }
        public int Revision { get; set; }


        public LearningState State
        {
            get
            {
                if (Examed > MaxExamScore)
                    return LearningState.Done;
                return (LearningState)(Examed / 2);
            }
        }

        public void OnExamPassed()
        {
            PassedScore++;
            LastExam = DateTime.Now;
            Examed++;
            AggregateScore = PassedScore;
        }

        public void OnExamFailed()
        {
            if (PassedScore > PenaltyScore)
                PassedScore = PenaltyScore;

            PassedScore = (int)Math.Round(PassedScore * 0.7);
            if (PassedScore < 0)
                PassedScore = 0;

            LastExam = DateTime.Now;
            Examed++;
            AggregateScore = PassedScore;
        }


        public void UpdateAgingAndRandomization()
        {
            double res = AggedScore;

            //probability reduces by reducingPerPointFactor for every res point
            var p = 100 / Math.Pow(ReducingPerPointFactor, res);

            //Randomize
            var rndFactor = Math.Pow(1.5, RandomTools.RandomNormal(0, 1));
            p = p * rndFactor;
            AggregateScore = p;

        }
    }
}