using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SayWhat.MongoDAL.Words
{
    public class UserWord {
        public ObjectId     Id       { get; set; }
        [BsonElement(UserWordsRepo.UserIdFieldName)]
        public ObjectId     UserId   { get; set; }
        public TranlationDirection Language { get; set; }
        public string   Word { get; set; }
        public double AbsoluteScore { get; set; }
        
        [BsonElement(UserWordsRepo.CurrentRatingFieldName)]
        public double CurrentRate   { get; set; }
        public int QuestionsPassed { get; set; }
        public int ExamsPassed { get; set; }
        public UserWordTranslation[] Translations { get; set; }
        public int ExamsCount { get; set; }
        public DateTime? LastExam { get; set; }
        [BsonElement(UserWordsRepo.PassedScoreFieldName)]
        public int PassedScore { get; set; }
        public int Examed { get; set; }
        public double AggregateScore { get; set; }
        public UserWordType Type { get; set; }
    }

    public enum UserWordType
    {
        UsualWord,
        AutoPhrase
    }
}