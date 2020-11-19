using MongoDB.Bson;

namespace SayWhat.MongoDAL.Words
{
    public class UserWordTranslation {
        public ObjectId Id { get; set;}
        public string Word   { get; set; }
        public string Transcription   { get; set; }
        public UserWordTranslationExample[] Examples { get; set; }
    }
}