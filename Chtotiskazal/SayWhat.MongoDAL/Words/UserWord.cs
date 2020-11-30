using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SayWhat.MongoDAL.Words
{
    public class UserWord {
        // ReSharper disable once InconsistentNaming
        public ObjectId     Id       { get; set; }
        [BsonElement(UserWordsRepo.UserIdFieldName)]
        public ObjectId     UserId   { get; set; }
        [BsonElement("l")]
        public TranlationDirection Language { get; set; }
        [BsonElement(UserWordsRepo.OriginWordFieldName)]
        public string   Word { get; set; }
      
        /// <summary>
        /// Current words rate. Include [AbsoluteScore] [AgingFactor] [Randomization]
        /// Index for exam selecting
        /// </summary>
        [BsonElement(UserWordsRepo.CurrentScoreFieldName)]
        public double CurrentScore   { get; set; }
        /// <summary>
        /// Absolute words score.
        /// </summary>
        [BsonElement(UserWordsRepo.AbsoluteScoreFieldName)]
        public double AbsoluteScore { get; set; }
        /// <summary>
        /// Number of correctly answered questions 
        /// </summary>
        [BsonElement("qp")]
        public int QuestionPassed { get; set; }
        /// <summary>
        /// Number of asked question 
        /// </summary>
        [BsonElement(UserWordsRepo.QuestionAskedFieldName)]
        public int QuestionAsked { get; set; }
        /// <summary>
        /// Last time the question was asked
        /// </summary>
        [BsonElement("askt")]
        public DateTime? LastQuestionTimestamp { get; set; }
        /// <summary>
        /// Last updated
        /// </summary>
        [BsonElement(UserWordsRepo.LastUpdateScoreTime)]
        public DateTime  ScoreUpdatedTimestamp { get; set; } = DateTime.Now;

        [BsonElement("tr")]
        public UserWordTranslation[] Translations { get; set; }
        [BsonElement("type")]
        public UserWordType Type { get; set; } = UserWordType.UsualWord;

        public override string ToString() => $"{Word} {CurrentScore} updated {ScoreUpdatedTimestamp}";
    }

    public enum UserWordType
    {
        // word
        UsualWord = 1,
        // automaticly added phrase
        AutoPhrase =2
    }
}