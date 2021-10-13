using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SayWhat.MongoDAL.QuestionMetrics;
using SayWhat.MongoDAL.Users;

namespace SayWhat.MongoDAL.WordKits {

[BsonIgnoreExtraElements]
public class LearningSet {
    public ObjectId Id { get; set; } = new ObjectId();
    [BsonElement("words")] public List<WordInLearningSet> Words { get; set; }
    [BsonElement("name")] public string Name { get; set; }
    [BsonElement("enabled")] public bool Enabled { get; set; }
    [BsonElement("used")] public int Used { get; set; }
    [BsonElement("passed")] public int Passed { get; set; }

    public void RegisterUsage(UserModel user, int offset) {
        user.TrainingSets ??= new List<UserTrainSet>();
        var alreadyContains = user.TrainingSets.FirstOrDefault(t => t.KitId == Id);
        if (alreadyContains == null)
        {
            alreadyContains = new UserTrainSet { KitId = Id };
            user.TrainingSets.Add(alreadyContains);
        }

        alreadyContains.LastSeenWordOffset = offset % Words.Count;
    }
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