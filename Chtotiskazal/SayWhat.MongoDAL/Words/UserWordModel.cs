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
        

        public UserWordModel(ObjectId userId, string word, string translation, double rate = 0)
        {
            _userId = userId;
            _word = word;
            _currentOrderScore = rate;
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

        public UserWordModel() { }

        #region mongo fields

        /// <summary>
        /// Last time the question was asked
        /// </summary>
        [BsonElement(UserWordsRepo.LastQuestionAskedTimestampFieldName)] 
        private DateTime? _lastQuestionTimestamp;

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
        private double _currentOrderScore;

        /// <summary>
        /// Absolute words score.
        /// </summary>
        [BsonElement(UserWordsRepo.AbsoluteScoreFieldName)]
        private double _absoluteScore;

        /// <summary>
        /// Number of correctly answered questions 
        /// </summary>
        [BsonElement(UserWordsRepo.QuestionPassedFieldName)] private int _questionPassed;

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
        public double CurrentOrderScore => _currentOrderScore;
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
        public UserWordScore Score => new UserWordScore(_absoluteScore, LastQuestionTimestamp??DateTime.Now);
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
            if (_absoluteScore > WordLeaningGlobalSettings.PenaltyScore)
                _absoluteScore = WordLeaningGlobalSettings.PenaltyScore;

            _absoluteScore = (int) Math.Round(AbsoluteScore * 0.7);
            if (_absoluteScore < 0)
                _absoluteScore = 0;

            _questionAsked++;
            _lastQuestionTimestamp =_scoreUpdatedTimestamp = DateTime.Now;

            UpdateCurrentScore();
        }

        public void UpdateCurrentScore()
        {
            var probability = Math.Pow(
                WordLeaningGlobalSettings.ReducingPerPointFactor, 
                Score.AgedScore);

            //normal randomize the probability 
            var rndFactor = Math.Pow(1.5, Rand.RandomNormal(0, 1));
            probability *= rndFactor;

            _currentOrderScore = probability;
            _scoreUpdatedTimestamp = DateTime.Now;
        }

        public void AddTranslations(List<UserWordTranslation> newTranslates)
        {
            newTranslates.AddRange(Translations);
            _translations = newTranslates.ToArray();
        }

        public override string ToString() => $"{Word} {CurrentOrderScore} updated {ScoreUpdatedTimestamp}";
    }
}