using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SayWhat.MongoDAL.Dictionary
{
    public class DictionaryTranslation {
        
        // ReSharper disable once InconsistentNaming
        [BsonElement("w")]
        public string   Word    { get; set; }
        [BsonElement("l")]
        public Language Language    { get; set; }
        [BsonElement("e")]
        public DictionaryReferenceToExample[] Examples  { get; set; }
    }
}