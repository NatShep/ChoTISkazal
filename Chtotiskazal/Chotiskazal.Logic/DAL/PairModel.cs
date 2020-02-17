using System;
using System.Collections.Generic;
using System.Linq;

namespace Dic.Logic.DAL
{
    public class PairModel
    {
        public const int MaxExamScore = 10;
        public const int PenaltyScore = 9;

        private const int ExamFailedPenalty = 2;
        private const double AgingFactor = 1.5;
        
        public double AggregateScore { get; set; }
        public long Id { get; set; }
        public int PassedScore { get; set; }
        public DateTime LastExam { get; set; }
        public int Examed { get; set; }
        public DateTime Created { get; set; }
        public string OriginWord { get; set; }
        public string Translation { get; set; }
        public string Transcription { get; set; }
        public List<Phrase> Phrases { get; set; }
        public IEnumerable<string> GetTranslations() => Translation.Split(',').Select(s => s.Trim());
        public void SetTranslations(string[] translations)
        {
            Translation = string.Join(", ",translations);
        }
        public IEnumerable<Phrase> GetPhraseForTranslations(IEnumerable<string> translations)
        {
            foreach (var translation in translations)
            {
                foreach (var origin in Phrases.Where(p=>p.Translation==translation))
                {
                    yield return origin;
                }
            }
        }
        public LearningState State
        {
            get
            {
                if (Examed > PairModel.MaxExamScore)
                    return LearningState.Done;
                return (LearningState) (Examed / 2);
            }
        }

        public static PairModel CreatePair(
            string originWord, 
            string translationWord, 
            string transcription, 
            Phrase[] phrases =null)
        {
            return new PairModel
            {
                Created = DateTime.Now,
                LastExam = DateTime.Now,
                OriginWord = originWord,
                Transcription = transcription,
                Translation = translationWord,
                Phrases =  phrases?.ToList(),
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

            //probability reduces 1.5 for every res point
            var p = 100 / Math.Pow(1.5, res);

            //Randomize
            var rndFactor = Math.Pow(1.5, RandomTools.RandomNormal(0, 2));
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