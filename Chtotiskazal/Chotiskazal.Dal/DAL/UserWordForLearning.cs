﻿using System;
using System.Collections.Generic;
using System.Linq;
 using Chotiskazal.DAL;
 using Chotiskazal.Dal.Enums;
 using Chotiskazal.DAL.Services;
 using Chotiskazal.LogicR;
// ReSharper disable MemberCanBePrivate.Global

 namespace Chotiskazal.DAL
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
             LastExam = null;
         }


         public IEnumerable<string> GetTranslations() => UserTranslations.Split(',').Select(s => s.Trim());

         public void SetTranslation(string[] translations) => UserTranslations = string.Join(", ", translations);

         public IEnumerable<int> GetPhrasesId()
         {
             List<int> phrasesId = new List<int>();
             foreach (var phraseId in PhrasesIds.Split(',').Select(s => s.Trim()))
             {
                 phrasesId.Add(int.Parse(phraseId));
             }
             return phrasesId;
         }

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
                 PhrasesIds = "",
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
         public double AggedScore ()
         {
             if (LastExam!=null)
                 return Math.Max(0, PassedScore - (DateTime.Now - LastExam.Value).TotalDays / AgingFactor);
             return 0;
         }

         public void UpdateAgingAndRandomization()
         {
             double res = AggedScore();

             //probability reduces by reducingPerPointFactor for every res point
             var p = 100 / Math.Pow(ReducingPerPointFactor, res);

             //Randomize
             var rndFactor = Math.Pow(1.5, RandomTools.RandomNormal(0, 1));
             p = p * rndFactor;
             AggregateScore = p;
         }
     }
 }