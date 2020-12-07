using System;
using System.Collections.Generic;
using System.Linq;
using SayWhat.Bll.Dto;
using SayWhat.Bll.Yapi;
using SayWhat.MongoDAL.Examples;
using SayWhat.MongoDAL.Words;

namespace SayWhat.Bll
{
    public class UserWordModel
    {
        private readonly UserWord _entity;

        public UserWordModel(UserWord entity)
        {
            _entity = entity;
        }
        
         public const int MaxExamScore = 10;
         public const int PenaltyScore = 9; 
         public const double AgingFactor = 1;
         public const double ReducingPerPointFactor = 1.7;

         public IEnumerable<string> GetTranslations() => _entity.Translations.Select(t => t.Word);

         public IEnumerable<string> GetTranscription() => _entity.Translations.Select(t => t.Transcription);

         public IEnumerable<UserWordTranslation> GetUserWordTranslations() => _entity.Translations;

         public void SetTranslation(string[] translations) =>
             _entity.Translations = translations
                 .Select(t => new UserWordTranslation
                 {
                    Word = t
                 }).ToArray();

         public LearningState State
         {
             get
             {
                 if (_entity.AbsoluteScore > MaxExamScore)
                     return LearningState.Done;
                 return (LearningState) (_entity.AbsoluteScore / 2);
             }
         }

         public UserWord Entity => _entity;

         public bool HasAnyPhrases => _entity.Translations.Any(t => t.Examples?.Any()==true);
         public string Word => _entity.Word;
         
         public Example GetRandomExample() =>
             Phrases
                 .ToList()
                 .GetRandomItem();

         public IEnumerable<Example> Phrases =>
             _entity.Translations
                 .SelectMany(t => t.Examples)
                 .Select(t => t.ExampleOrNull)
                 .Where(e => e != null);
         
         public double AbsoluteScore => _entity.AbsoluteScore;
         public double CurrentScore => _entity.CurrentScore;
         public int QuestionPassed => _entity.QuestionPassed;
         public int QuestionAsked => _entity.QuestionAsked;
         public DateTime? LastExam => _entity.LastQuestionTimestamp;
         public string TranslationAsList => string.Join(", ", _entity.Translations.Select(t => t.Word));

         public void OnExamPassed()
         {
             _entity.AbsoluteScore++;
             _entity.LastQuestionTimestamp = DateTime.Now;
             _entity.QuestionAsked++;
             _entity.QuestionPassed++;
             _entity.ScoreUpdatedTimestamp = DateTime.Now;
             UpdateCurrentScore();
         }

         public void OnExamFailed()
         {
             if (_entity.AbsoluteScore > PenaltyScore)
                 _entity.AbsoluteScore = PenaltyScore;

             _entity.AbsoluteScore = (int) Math.Round(_entity.AbsoluteScore * 0.7);
             if (_entity.AbsoluteScore < 0)
                 _entity.AbsoluteScore = 0;

             _entity.LastQuestionTimestamp = DateTime.Now;
             _entity.QuestionAsked++;
             
             _entity.ScoreUpdatedTimestamp = DateTime.Now;
             UpdateCurrentScore();
         }

         //res reduces for 1 point per AgingFactor days
         private double GetAgedScore ()
         {
             //if there were no question yet - return some big number as if the word was asked long time ago  
             if (_entity.LastQuestionTimestamp == null) return 0;
             return Math.Max(0, _entity.AbsoluteScore - (DateTime.Now - _entity.LastQuestionTimestamp.Value).TotalDays 
                                / AgingFactor);
         }

         public void UpdateCurrentScore()
         {
             double res = GetAgedScore();

             //probability reduces by reducingPerPointFactor for every res point
             var p = Math.Pow(ReducingPerPointFactor, res);

             //Randomize
             var rndFactor = Math.Pow(1.5, Random.RandomNormal(0, 1));
             p *= rndFactor;
             _entity.CurrentScore = p;
             _entity.ScoreUpdatedTimestamp = DateTime.Now;
         }

        

         public void AddTranslations(List<UserWordTranslation> newTranslates)
         {
             newTranslates.AddRange(_entity.Translations);
             _entity.Translations = newTranslates.ToArray();
         }

         public override string ToString() => _entity.ToString();
    }
}