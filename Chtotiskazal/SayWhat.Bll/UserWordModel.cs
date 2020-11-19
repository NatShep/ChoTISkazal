using System;
using System.Collections.Generic;
using System.Linq;
using SayWhat.Bll.Dto;
using SayWhat.Bll.Yapi;
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

         public IEnumerable<string> GetTranslations() => _entity.Translations.Select(t => t.Word);

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
                 if (_entity.Examed > MaxExamScore)
                     return LearningState.Done;
                 return (LearningState) (_entity.Examed / 2);
             }
         }

         public UserWord Entity => _entity;

         public bool HasAnyPhrases => _entity.Translations.Any(t => t.Examples.Any());
         public string Word => _entity.Word;
         public IEnumerable<WordExample> Phrases => 
             _entity.Translations.SelectMany(t => t.Examples.Select(e=>new WordExample(Word,t.Word,e.Origin, e.Translation)));
         public IEnumerable<string> TranlatedPhrases => _entity.Translations.SelectMany(t => t.Examples).Select(e=>e.Translation);
         public IEnumerable<string> OriginPhrases => _entity.Translations.SelectMany(t => t.Examples).Select(e=>e.Origin);
         public int PassedScore => _entity.PassedScore;
         public double AggregateScore => _entity.AggregateScore;
         public int Examed => _entity.Examed;
         public DateTime? LastExam => _entity.LastExam;
         public string TranlationAsList => string.Join(", ", _entity.Translations.Select(t => t.Word));

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

         public WordExample GetRandomExample()
         {
             var list = _entity.Translations.SelectMany(translation => translation.Examples.Select(
                 example=>new{translation, example}))
                 .ToList();
             var res =  list.GetRandomItem();
             return new WordExample(_entity.Word, res.translation.Word, res.example.Origin, res.example.Translation);
         }

         public void AddTranslations(List<UserWordTranslation> newTranslates)
         {
             newTranslates.AddRange(_entity.Translations);
             _entity.Translations = newTranslates.ToArray();
         }
    }

    public class WordExample
    {
        public string[] OriginWords => OriginPhrase.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        public int OriginWordsCount => OriginPhrase.Count(c=>c==' ');
        public int TranslationWordsCount => OriginPhrase.Count(c=>c==' ');

        public WordExample(string originWord, string wordTranslation, string originPhrase, string phraseTranslation)
        {
            OriginWord = originWord;
            WordTranslation = wordTranslation;
            OriginPhrase = originPhrase;
            PhraseTranslation = phraseTranslation;
        }

        public string OriginWord { get; }
        public string WordTranslation { get; }
        public string OriginPhrase { get; }
        public string PhraseTranslation { get; }
    }
}