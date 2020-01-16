using System;

namespace Dic.Logic.DAL
{
    public class PairModel
    {
        private static Random rnd = new Random(DateTime.Now.Millisecond);
        public const int MaxExamScore = 5;
        private const int ExamFailedPenalty = 2;
        private const double RandomFactor = 1;
        private const double AgingFactor = 7;
        
        public double AggregateScore { get; set; }
        public long Id { get; set; }
        public int PassedScore { get; set; }
        public DateTime LastExam { get; set; }
        public int Examed { get; set; }
        public DateTime Created { get; set; }
        public string OriginWord { get; set; }
        public string Translation { get; set; }
        public string Transcription { get; set; }

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
            if (PassedScore > MaxExamScore)
                PassedScore = MaxExamScore;
            
            PassedScore -= ExamFailedPenalty;
            if (PassedScore < 0)
                PassedScore = 0;

            LastExam = DateTime.Now;
            Examed++;
            AggregateScore = PassedScore;
        }

        public void UpdateAgingAndRandomization()
        {
            var res = (double)PassedScore;
            res -= (DateTime.Now - LastExam).TotalDays / AgingFactor;
            res += RandomFactor * (rnd.NextDouble() - 0.5);
            if (res < 0)
                res = 0;

            AggregateScore = res;

        }
    }
}