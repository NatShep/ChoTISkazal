using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SayWhat.MongoDAL.Examples;

namespace SayWhat.MongoDAL.Dictionary
{
    public class DictionaryReferenceToExample {

        public ObjectId ExampleId { get; set; }
        [BsonIgnore] public Example ExampleOrNull { get; set; }
    }
}