using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SayWa.MongoDAL.Words
{
    public class UserWord {
        public ObjectId     Id       { get; set; }
        [BsonElement(UserWordsRepository.UserIdFieldName)]
        public ObjectId     UserId   { get; set; }
        public TranlationDirection Language { get; set; }
        public string   Word { get; set; }
        public double AbsoluteRate { get; set; }
        
        [BsonElement(UserWordsRepository.CurrentRatingFieldName)]
        public double CurrentRate   { get; set; }
        public int QuestionsPassed { get; set; }
        public int ExamsPassed { get; set; }
        public UserWordTranslation[] Translations { get; set; }
    }
}