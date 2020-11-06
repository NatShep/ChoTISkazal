﻿using System;
using System.Collections.Generic;
using System.Linq;
 using Chotiskazal.DAL;
 using Chotiskazal.Dal.Enums;
 using Chotiskazal.LogicR;

 namespace Chotiskazal.DAL
{
    public class PairMetric
    {
        public const int MaxExamScore = 10;
        public const int PenaltyScore = 9;

        private const int ExamFailedPenalty = 2;
        private const double AgingFactor = 1;
        public const double ReducingPerPointFactor = 1.7;
        
        
        public long MetricId { get; set; }
        public string EnWord { get; set; }
        public int UserId { get; set; }
        public DateTime Created { get; set; }

        public int PassedScore { get; set; }
        public double AggregateScore { get; set; }

        public DateTime LastExam { get; set; }
        public int Examed { get; set; }

        public int Revision { get; set; }





        public LearningState GetState()
        {
            if (Examed > PairMetric.MaxExamScore)
                return LearningState.Done;
            return (LearningState) (Examed / 2);
        }


        public static PairMetric CreateMetric(string enWord)
        {
            return new PairMetric
            {
                Created = DateTime.Now,
                LastExam = DateTime.Now,
                EnWord = enWord,
                Revision = 1,
            };
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
            
            PassedScore =  (int) Math.Round(PassedScore*0.7);
            if (PassedScore < 0)
                PassedScore = 0;

            LastExam = DateTime.Now;
            Examed++;
            AggregateScore = PassedScore;
        }

        //res reduces for 1 point per AgingFactor days
        public double AggedScore => Math.Max(0, PassedScore - (DateTime.Now - LastExam).TotalDays / AgingFactor);
        public void UpdateAgingAndRandomization()
        {
            double res = AggedScore;

            //probability reduces by reducingPerPointFactor for every res point
            var p = 100 / Math.Pow(ReducingPerPointFactor, res);

            //Randomize
            var rndFactor = Math.Pow(1.5, RandomTools.RandomNormal(0, 1));
            p = p*rndFactor ;
            AggregateScore = p;
        }
        
    }
}