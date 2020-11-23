using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SayWhat.MongoDAL.Examples
{
    public class Example
    {
        public Example()
        {
            Direction = TranlationDirection.EnRu;
        }
        public string[] OriginWords => OriginPhrase.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        [BsonId]
        public ObjectId Id { get; set; }
        public string OriginWord { get; set; }
        public string TranslatedWord { get; set; }
        public string OriginPhrase { get; set; }
        public string TranslatedPhrase { get; set; }
        public TranlationDirection Direction { get; set; }
    }
}