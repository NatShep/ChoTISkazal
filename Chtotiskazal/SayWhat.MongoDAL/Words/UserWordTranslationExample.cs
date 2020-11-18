using MongoDB.Bson;

namespace SayWhat.MongoDAL.Words
{
    public class UserWordTranslationExample
    {
        public ObjectId Id { get; set; }
        public string Origin { get;set; }
        public string Translation { get;set; }
    }
}