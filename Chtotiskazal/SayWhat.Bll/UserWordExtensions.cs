using System;
using Chotiskazal.DAL.Services;
using SayWhat.MongoDAL.Words;

namespace SayWhat.Bll
{
    public static class UserWordExtensions
    {
        public const int MaxExamScore = 10;
        public const int PenaltyScore = 9; 
        public const double AgingFactor = 1;
        public const double ReducingPerPointFactor = 1.7;
        
        public static void OnExamPassed(this UserWord model)
        {
            model.PassedScore++;
            model.LastExam = DateTime.Now;
            model.Examed++;
            model.AggregateScore = model.PassedScore;
        }

        public static void OnExamFailed(this UserWord model)
        {
            if (model.PassedScore > PenaltyScore)
                model.PassedScore = PenaltyScore;

            model.PassedScore = (int) Math.Round(model.PassedScore * 0.7);
            if (model.PassedScore < 0)
                model.PassedScore = 0;

            model.LastExam = DateTime.Now;
            model.Examed++;
            model.AggregateScore = model.PassedScore;
        }

        //res reduces for 1 point per AgingFactor days
        public static double AgedScore (this UserWord model)
        {
            if (model.LastExam!=null)
                return Math.Max(0, model.PassedScore - (DateTime.Now - model.LastExam.Value).TotalDays / AgingFactor);
            return 0;
        }

        public static void UpdateAgingAndRandomization(this UserWord model)
        {
            double res = model.AgedScore();

            //probability reduces by reducingPerPointFactor for every res point
            var p = 100 / Math.Pow(ReducingPerPointFactor, res);

            //Randomize
            var rndFactor = Math.Pow(1.5, RandomTools.RandomNormal(0, 1));
            p *= rndFactor;
            model.AggregateScore = p;
        }
    }
}