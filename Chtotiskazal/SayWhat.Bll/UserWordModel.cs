using System;
using System.Collections.Generic;
using System.Linq;
using Chotiskazal.Dal.DAL;
using Chotiskazal.DAL.Services;
using SayWhat.MongoDAL.Words;

namespace SayWhat.Bll
{
    public class UserWordModel
    {
        private UserWord _entity;

        public UserWordModel(UserWord entity)
        {
            _entity = entity;
        }
        
         public const int MaxExamScore = 10;
         public const int PenaltyScore = 9; 
         public const double AgingFactor = 1;
         public const double ReducingPerPointFactor = 1.7;

         public IEnumerable<string> GetTranslations() => _entity.Translations.Select(t => t.Translation);

         public void SetTranslation(string[] translations) =>
             _entity.Translations = translations
                 .Select(t => new UserWordTranslation
                 {
                    Translation = t
                 }).ToArray();

         public LearningState State
         {
             get
             {
                 if (_entity.Examed > MaxExamScore)
                     return LearningState.Done;
                 return (LearningState) (_entity.Examed / 2);
             }
         }

         public UserWord Entity => _entity;
         /*public static UserWordForLearning CreatePair(
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
         }*/

         public void OnExamPassed()
         {
             _entity.PassedScore++;
             _entity.LastExam = DateTime.Now;
             _entity.Examed++;
             _entity.AggregateScore = _entity.PassedScore;
         }

         public void OnExamFailed()
         {
             if (_entity.PassedScore > PenaltyScore)
                 _entity.PassedScore = PenaltyScore;

             _entity.PassedScore = (int) Math.Round(_entity.PassedScore * 0.7);
             if (_entity.PassedScore < 0)
                 _entity.PassedScore = 0;

             _entity.LastExam = DateTime.Now;
             _entity.Examed++;
             _entity.AggregateScore = _entity.PassedScore;
         }

         //res reduces for 1 point per AgingFactor days
         public double AgedScore ()
         {
             if (_entity.LastExam!=null)
                 return Math.Max(0, _entity.PassedScore - (DateTime.Now - _entity.LastExam.Value).TotalDays / AgingFactor);
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
             _entity.AggregateScore = p;
         }
    }
}