using MongoDB.Bson.Serialization.Attributes;

namespace SayWhat.MongoDAL.Dictionary;

public class DictionaryTranslationDbEntity {
    [BsonElement("w")] public string Word { get; set; }
    [BsonElement("l")] public Language Language { get; set; }
    [BsonElement("e")] public DictionaryReferenceToExample[] Examples { get; set; }
}