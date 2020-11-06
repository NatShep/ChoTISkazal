﻿using System;
using System.Collections.Generic;
using System.Linq;
 using Chotiskazal.DAL;
 using Chotiskazal.Dal.Enums;
 using Chotiskazal.LogicR;

 namespace Chotiskazal.DAL
 {
     public class UserWordForLearning
     {
         public const int MaxExamScore = 10;
         public const int PenaltyScore = 9;

         private const int ExamFailedPenalty = 2;
         private const double AgingFactor = 1;
         public const double ReducingPerPointFactor = 1.7;

         public long Id { get; set; }
         public int UserId { get; set; }
         public string EnWord { get; set; }
         public string UserTranslations { get; set; }
         public string Transcription { get; set; }
         public DateTime Created { get; set; }

         public string Phrases { get; set; }
         public int PassedScore { get; set; }
         public double AggregateScore { get; set; }

         public DateTime LastExam { get; set; }
         public int Examed { get; set; }
         public int Revision { get; set; }

      

         public IEnumerable<string> GetTranslations() => UserTranslations.Split(',').Select(s => s.Trim());

         public void SetTranslations(string[] translations) => UserTranslations = string.Join(", ", translations);

         public IEnumerable<string> GetPhrases => Phrases.Split(',').Select(s => s.Trim());


         public LearningState State
         {
             get
             {
                 if (Examed > UserWordForLearning.MaxExamScore)
                     return LearningState.Done;
                 return (LearningState) (Examed / 2);
             }
         }

         public static UserWordForLearning CreatePair(
             string originWord,
             string translationWord,
             string[] allMeanings,
             string transcription,
             Phrase[] phrases = null)
         {
             return new UserWordForLearning
             {
                 Created = DateTime.Now,
                 LastExam = DateTime.Now,
                 EnWord = originWord,
                 Transcription = transcription,
                 UserTranslations = translationWord,
                 Revision = 1,
                 Phrases = "",
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
         public double AggedScore => Math.Max(0, PassedScore - (DateTime.Now - LastExam).TotalDays / AgingFactor);

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