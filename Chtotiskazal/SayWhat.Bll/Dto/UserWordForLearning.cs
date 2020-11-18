using System;
using System.Collections.Generic;
using System.Linq;
using Chotiskazal.DAL.Services;

// ReSharper disable MemberCanBePrivate.Global

 namespace Chotiskazal.Dal.DAL
 {
     public class UserWordForLearning
     {
         public const int MaxExamScore = 10;
         public const int PenaltyScore = 9; 
         public const double AgingFactor = 1;
         public const double ReducingPerPointFactor = 1.7;

         public long Id { get; set; }
         public int UserId { get; set; }
         public string EnWord { get; set; }
         public string UserTranslations { get; set; }
         public string Transcription { get; set; }
         public DateTime Created { get; set; }
         
         //TODO Check How Add PhrasesIds. Do we need this param?
         public string PhrasesIds { get; set; }
         public bool IsPhrase { get; set; }
         public int PassedScore { get; set; }
         public double AggregateScore { get; set; }
         public DateTime? LastExam { get; set; }
         public int Examed { get; set; }
         public int Revision { get; set; }


         public List<Phrase> Phrases { get; set; }

         public UserWordForLearning()
         {
             Phrases = new List<Phrase>();
             Created = DateTime.Now;
             Revision = 1;
             LastExam = null;
             Transcription = "";
         }


         public IEnumerable<string> GetTranslations() => UserTranslations.Split(',').Select(s => s.Trim());

         public void SetTranslation(string[] translations) => UserTranslations = string.Join(", ", translations);

         public List<int> GetPhrasesId()
         {
             if (PhrasesIds!="")
                return PhrasesIds.Split(',').Select(s => s.Trim()).Select(int.Parse).ToList();
             return new List<int>();
             
         }

         public LearningState State
         {
             get
             {
                 if (Examed > MaxExamScore)
                     return LearningState.Done;
                 return (LearningState) (Examed / 2);
             }
         }
         public static UserWordForLearning CreatePair(
             string originWord,
             string translationWord,
             string transcription,
             List<Phrase> phrases = null,
             int[] phrasesId=null,
             bool isPhrase=false)
         {
             var ids = "";
             if (phrasesId != null)
                 ids = string.Join(",", phrasesId);
             return new UserWordForLearning
             {
                 Created = DateTime.Now,
                 LastExam = null,
                 EnWord = originWord,
                 Transcription = transcription ?? "[]",
                 UserTranslations = translationWord,
                 Revision = 1,
                 PhrasesIds = ids,
                 Phrases = phrases ?? new List<Phrase>(),
                 IsPhrase = isPhrase
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

             PassedScore = (int) Math.Round(PassedScore * 0.7);
             if (PassedScore < 0)
                 PassedScore = 0;

             LastExam = DateTime.Now;
             Examed++;
             AggregateScore = PassedScore;
         }

         //res reduces for 1 point per AgingFactor days
         public double AgedScore ()
         {
             if (LastExam!=null)
                 return Math.Max(0, PassedScore - (DateTime.Now - LastExam.Value).TotalDays / AgingFactor);
             return 0;
         }

         public void UpdateAgingAndRandomization()
         {
             double res = AgedScore();

             //probability reduces by reducingPerPointFactor for every res point
             var p = 100 / Math.Pow(ReducingPerPointFactor, res);

             //Randomize
             var rndFactor = Math.Pow(1.5, RandomTools.RandomNormal(0, 1));
             p *= rndFactor;
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