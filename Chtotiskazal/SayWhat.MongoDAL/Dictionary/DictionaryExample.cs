using MongoDB.Bson;

namespace SayWa.MongoDAL.Dictionary
{
    public class DictionaryExample {
        public ObjectId Id { get; set; }
        public string OriginExample      { get; set; }
        public string TranslationExample { get; set; }
    }
}