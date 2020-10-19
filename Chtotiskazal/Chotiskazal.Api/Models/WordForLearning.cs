﻿using System;
using System.Collections.Generic;
using System.Linq;
 using Chotiskazal.Dal;
 using Chotiskazal.DAL;
 using Chotiskazal.DAL.ModelsForApi;
 using Chotiskazal.LogicR;

 namespace Chotiskazal.Api.Models
{
    public class WordForLearning
    {
        public const int MaxExamScore = 10;
        public const int PenaltyScore = 9;

        private const int ExamFailedPenalty = 2;
        private const double AgingFactor = 1;
        public const double ReducingPerPointFactor = 1.7;

        public int Id { get; set; }
        
        public int MetricId { get; set; }

        public double AggregateScore { get; set; }
        public int PassedScore { get; set; }
        public DateTime LastExam { get; set; }
        public int Examed { get; set; }
        public DateTime Created { get; set; }
        public string OriginWord { get; set; }
        public string Translations { get; set; }
        public string Transcription { get; set; }
     //   public string AllMeanings { get; set; }
        public int Revision { get; set; }
        public List<PhraseForApi> Phrases { get; set; }
     
        public string[] AllMeanings { get; set; }

        public WordForLearning(UserPair userPair, WordDictionary pairFromDictionary, QuestionMetric questionMetric,
            string[] allMeanings, string[] allTranslationsOfWordForUser)
        {
            
            MetricId = userPair.MetricId;
            AggregateScore = questionMetric.AggregateScoreBefore;
            PassedScore = questionMetric.PassedScore;
            LastExam = questionMetric.LastExam;
            Examed = questionMetric.Examed;
            Created = userPair.Created;
            OriginWord = pairFromDictionary.EnWord;
            SetTranslations(allTranslationsOfWordForUser);
            Transcription = pairFromDictionary.Transcription;
            AllMeanings = allMeanings;
            Phrases=new List<PhraseForApi>();
            foreach (var phrase in pairFromDictionary.Phrases)
                Phrases.Add(phrase.MapToApiPhrase());
            Revision = 1;

        }
    

        public string[] GetAllMeanings() => AllMeanings;
     
        /*{
            if (!string.IsNullOrWhiteSpace(AllMeanings))
                return AllMeanings.Split(";;");
            else
            {
                return Translation.Split(',').Select(s => s.Trim()).ToArray();
            }
        }*/
        
        public IEnumerable<string> GetTranslations() => Translations.Split(',').Select(s => s.Trim());
        public void SetTranslations(string[] translations) => Translations = string.Join(", ",translations);
        


    public IEnumerable<PhraseForApi> GetPhraseForTranslations(IEnumerable<string> translations)
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
                if (Examed > WordForLearning.MaxExamScore)
                    return LearningState.Done;
                return (LearningState) (Examed / 2);
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