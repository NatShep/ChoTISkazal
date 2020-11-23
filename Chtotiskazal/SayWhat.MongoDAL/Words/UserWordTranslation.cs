using MongoDB.Bson;

namespace SayWhat.MongoDAL.Words
{
    public class UserWordTranslation {
        public UserWordTranslation()
        {
            Examples = new UserWordTranslationReferenceToExample[0];
        }
        public string Word   { get; set; }
        public string Transcription   { get; set; }
        public UserWordTranslationReferenceToExample[] Examples { get; set; }
    }
}