using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SayWhat.MongoDAL.Words
{
    public class UserWordTranslation {
        public UserWordTranslation()
        {
            Examples = new UserWordTranslationReferenceToExample[0];
        }
        [BsonElement("w")]
        public string Word   { get; set; }
        [BsonElement("ts")]
        public string Transcription   { get; set; }
        [BsonElement("e")]
        public UserWordTranslationReferenceToExample[] Examples { get; set; }
    }
}