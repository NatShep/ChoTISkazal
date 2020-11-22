using MongoDB.Bson;

namespace SayWhat.MongoDAL.Examples
{
    public class Example
    {
        public ObjectId Id { get; set; }
        public string OriginWord { get; set; }
        public string TranslatedWord { get; set; }
        public string OriginExample { get; set; }
        public string TranslatedExample { get; set; }
    }
}