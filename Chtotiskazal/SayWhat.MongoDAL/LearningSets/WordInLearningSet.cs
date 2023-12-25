using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SayWhat.MongoDAL.LearningSets;

[BsonIgnoreExtraElements]
public class WordInLearningSet
{
    [BsonElement("w")] public string Word { get; set; }

    [BsonElement("trs")] public string[] AllowedTranslations { get; set; }
    [BsonElement("es")] public ObjectId[] AllowedExamples { get; set; }
}