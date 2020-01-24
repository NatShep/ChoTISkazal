using System;

namespace Dic.Logic.DAL
{
    public class PairModel
    {
        public const int MaxExamScore = 10;
        public const int PenaltyScore = 9;

        private const int ExamFailedPenalty = 2;
        private const double AgingFactor = 3;
        
        public double AggregateScore { get; set; }
        public long Id { get; set; }
        public int PassedScore { get; set; }
        public DateTime LastExam { get; set; }
        public int Examed { get; set; }
        public DateTime Created { get; set; }
        public string OriginWord { get; set; }
        public string Translation { get; set; }
        public string Transcription { get; set; }

        public LearningState State
        {
            get
            {
                if (Examed > PairModel.MaxExamScore)
                    return LearningState.Done;
                return (LearningState) (Examed / 2);
            }
        }

        public static PairModel CreatePair(string originWord, string translationWord, string transcription)
        {
            return new PairModel
            {
                Created = DateTime.Now,
                LastExam = DateTime.Now,
                OriginWord = originWord,
                Transcription = transcription,
                Translation = translationWord,
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

        public void UpdateAgingAndRandomization()
        {
            var res = (double)PassedScore;
            //res reduces for 1 point per AgingFactor days
            res -= (DateTime.Now - LastExam).TotalDays / AgingFactor;
            if (res < 0)
                res = 0;

            //probability reduces twice for every res point
            var p = 100 / Math.Pow(2, res);

            //Randomize
            var rndFactor = Math.Pow(2, RandomTools.RandomNormal(0, 1));
            p = p*rndFactor ;
            AggregateScore = p;

        }
        
    }

    public enum LearningState
    {
        New = 0,
        Familiar = 1,
        Known = 2,
        NotSure = 3,
        PreLearned = 4,
        Done = 5,
    }
}