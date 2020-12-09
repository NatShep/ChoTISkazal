using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SayWhat.MongoDAL.Examples;
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable ConvertToAutoProperty
// ReSharper disable NotAccessedField.Local
// ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable 414

namespace SayWhat.MongoDAL.Words
{
    [BsonIgnoreExtraElements]
    public class UserWordModel
    {
        public const int MaxExamScore = 10;
        public const int PenaltyScore = 9;
        public const double AgingFactor = 1;
        public const double ReducingPerPointFactor = 1.7;

        public UserWordModel(ObjectId userId, string word, string translation, double rate = 0)
        {
            _userId = userId;
            _word = word;
            _currentScore = rate;
            _absoluteScore = rate;
            _translations = new[]
            {
                new UserWordTranslation(translation),
            };
        }

        public UserWordModel(ObjectId id, string word, TranslationDirection direction, double absScore,
            UserWordTranslation translation)
        {
            _userId = id;
            _word = word;
            _translationDirection = direction;
            _absoluteScore = absScore;
            _translations = new[] {translation};
        }

        public UserWordModel()
        {
        }

        #region mongo fields

        /// <summary>
        /// Last time the question was asked
        /// </summary>
        [BsonElement("askt")] private DateTime? _lastQuestionTimestamp;

        public ObjectId Id { get; set; }

        [BsonElement(UserWordsRepo.UserIdFieldName)]
        private ObjectId _userId;

        [BsonElement("l")] private TranslationDirection _translationDirection;

        [BsonElement(UserWordsRepo.OriginWordFieldName)]
        private string _word;

        /// <summary>
        /// Current words rate. Include [AbsoluteScore] [AgingFactor] [Randomization]
        /// Index for exam selecting
        /// </summary>
        [BsonElement(UserWordsRepo.CurrentScoreFieldName)]
        private double _currentScore;

        /// <summary>
        /// Absolute words score.
        /// </summary>
        [BsonElement(UserWordsRepo.AbsoluteScoreFieldName)]
        private double _absoluteScore;

        /// <summary>
        /// Number of correctly answered questions 
        /// </summary>
        [BsonElement("qp")] private int _questionPassed;

        /// <summary>
        /// Number of asked question 
        /// </summary>
        [BsonElement(UserWordsRepo.QuestionAskedFieldName)]
        private int _questionAsked;

        [BsonElement("tr")] private UserWordTranslation[] _translations;

        [BsonElement("t")] [BsonDefaultValue(UserWordType.UsualWord)] [BsonIgnoreIfDefault]
        private UserWordType _type = UserWordType.UsualWord;

        /// <summary>
        /// Last updated
        /// </summary>
        [BsonElement(UserWordsRepo.LastUpdateScoreTime)]
        private DateTime _scoreUpdatedTimestamp = DateTime.Now;

        #endregion

        public string Word => _word;
        public double CurrentScore => _currentScore;
        public double AbsoluteScore => _absoluteScore;
        public int QuestionPassed => _questionPassed;
        public int QuestionAsked => _questionAsked;
        public DateTime? LastQuestionTimestamp => _lastQuestionTimestamp;
        public DateTime ScoreUpdatedTimestamp => _scoreUpdatedTimestamp;

        public UserWordTranslation[] Translations
        {
            get => _translations;
            set => _translations = value;
        }

        public bool HasAnyExamples => Translations.Any(t => t.Examples?.Any() == true);
        public DateTime? LastExam => LastQuestionTimestamp;
        public string TranslationAsList => string.Join(", ", Translations.Select(t => t.Word));
        public IEnumerable<string> AllTranslations => Translations.Select(t => t.Word);

        public IEnumerable<Example> Examples =>
            Translations
                .SelectMany(t => t.Examples)
                .Select(t => t.ExampleOrNull)
                .Where(e => e != null);

        public Example GetRandomExample() =>
            Examples
                .ToList()
                .GetRandomItem();

        public void OnQuestionPassed()
        {
            _absoluteScore++;
            _lastQuestionTimestamp = DateTime.Now;
            _questionAsked++;
            _questionPassed++;
            _scoreUpdatedTimestamp = DateTime.Now;
            UpdateCurrentScore();
        }

        public void OnQuestionFailed()
        {
            if (_absoluteScore > PenaltyScore)
                _absoluteScore = PenaltyScore;

            _absoluteScore = (int) Math.Round(AbsoluteScore * 0.7);
            if (_absoluteScore < 0)
                _absoluteScore = 0;

            _lastQuestionTimestamp = DateTime.Now;
            _questionAsked++;

            _scoreUpdatedTimestamp = DateTime.Now;
            UpdateCurrentScore();
        }

        //res reduces for 1 point per AgingFactor days
        private double GetAgedScore()
        {
            //if there were no question yet - return some big number as if the word was asked long time ago  
            if (LastQuestionTimestamp == null) return 0;
            return Math.Max(0, AbsoluteScore - (DateTime.Now - LastQuestionTimestamp.Value).TotalDays
                / AgingFactor);
        }

        public void UpdateCurrentScore()
        {
            double res = GetAgedScore();

            //probability reduces by reducingPerPointFactor for every res point
            var p = Math.Pow(ReducingPerPointFactor, res);

            //Randomize
            var rndFactor = Math.Pow(1.5, Rand.RandomNormal(0, 1));
            p *= rndFactor;
            _currentScore = p;
            _scoreUpdatedTimestamp = DateTime.Now;
        }

        public void AddTranslations(List<UserWordTranslation> newTranslates)
        {
            newTranslates.AddRange(Translations);
            _translations = newTranslates.ToArray();
        }

        public override string ToString() => $"{Word} {CurrentScore} updated {ScoreUpdatedTimestamp}";
    }
}