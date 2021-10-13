using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SayWhat.MongoDAL.QuestionMetrics;

namespace SayWhat.MongoDAL.WordKits {

[BsonIgnoreExtraElements]
public class LearningSetModel {
    public ObjectId Id { get; set; } = new ObjectId();
    [BsonElement("words")] public List<WordInLearningSet> Words { get; set; }
    [BsonElement("name")] public string Name { get; set; }
    [BsonElement("enabled")] public bool Enabled { get; set; }
    [BsonElement("used")] public int Used { get; set; }
    [BsonElement("passed")] public int Passed { get; set; }
}

[BsonIgnoreExtraElements]
public class WordInLearningSet {
    public WordInLearningSet() { Id = ObjectId.GenerateNewId(); }
    public ObjectId Id { get; set; }

    [BsonElement("wid")] public ObjectId WordId { get; set; }

    [BsonElement("trs")] public string[] AllowedTranslations { get; set; }

    [BsonElement("es")] public ObjectId[] AllowedExamples { get; set; }
}

}