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
        public double AbsoluteRate { get; set; }
        
        [BsonElement(UserWordsRepo.CurrentRatingFieldName)]
        public double CurrentRate   { get; set; }
        public int QuestionsPassed { get; set; }
        public int ExamsPassed { get; set; }
        public UserWordTranslation[] Translations { get; set; }
    }
}