using Chotiskazal.Logic;
using Dic.Logic;
using Dic.Logic.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chotiskazal.DAL
{
    // хотим ли мы разделять слово-значение1, слово-значение2
    // или слово - все значения, помеченные юзером.
    class UsersPair
    {
        public const int MaxExamScore = 10;
        public const int PenaltyScore = 9;

        private const int ExamFailedPenalty = 2;
        private const double AgingFactor = 1;
        public const double ReducingPerPointFactor = 1.7;
        public double AggregateScore { get; set; }
        //res reduces for 1 point per AgingFactor days
        public double AggedScore => Math.Max(0, PassedScore - (DateTime.Now - LastExam).TotalDays / AgingFactor);


        public int Id { get; set; }
        public int UserId { get; set; }

        /*
         //Find words by UnicName(Word+Translate) in WordDictionary
         // if IsPhrase is True find words by UnicName(Word+Translate) in PhraseDictionary
         public string Word { get; set; }
         public string Translate { get; set; }
         public bool IsPhrase { get; set; }

         // if IsPhrase is True, Phrases=null
         public List<PhraseDictionary> Phrases { get; set; }

         */

        public int WordId { get; set; }

        //ExamsInfo for Word
        public int PassedScore { get; set; }
        public DateTime LastExam { get; set; }
        public DateTime PreviousExam { get; set; }
        public int Examed { get; set; }
        public int Revision { get; set; }
        public DateTime WordAdded { get; set; }
        public int ElaspedMs { get; set; }
        public double AggregateScoreBefore { get; set; }
        public int PhrasesCount { get; set; }
        public double PassedScoreBefore { get; set; }
        public int ExamsPassed { get; set; }
        public int Result { get; set; }
        public string Type { get; set; }
            

        public LearningState State
        {
            get
            {
                if (Examed > PairModel.MaxExamScore)
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

        public IEnumerable<PhraseDictionary> GetPhraseForTranslations(IEnumerable<string> translations)
        {
            foreach (var translation in translations)
            {
                foreach (var origin in Phrases.Where(p => p.Translation == translation))
                {
                    yield return origin;
                }
            }
        }

    }

    
}

