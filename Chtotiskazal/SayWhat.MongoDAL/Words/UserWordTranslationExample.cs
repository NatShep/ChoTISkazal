using MongoDB.Bson;

namespace SayWa.MongoDAL.Words
{
    public class UserWordTranslationExample
    {
        public ObjectId Id { get; set; }
        public string Origin { get;set; }
        public string Translation { get;set; }
    }
}