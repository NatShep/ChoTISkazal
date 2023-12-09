using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SayWhat.MongoDAL.Users;

namespace SayWhat.MongoDAL.LearningSets;

[BsonIgnoreExtraElements]
public class LearningSet {
    public ObjectId Id { get; set; } = new ObjectId();
    [BsonElement("words")] public List<WordInLearningSet> Words { get; set; }
    [BsonElement("shortName")] public string ShortName { get; set; }
    [BsonElement("enname")] public string EnName { get; set; }
    [BsonElement("runame")] public string RuName { get; set; }
    [BsonElement("enDesc")] public string EnDescription { get; set; }
    [BsonElement("ruDesc")] public string RuDescription { get; set; }

    [BsonElement("enabled")] public bool Enabled { get; set; }
    [BsonElement("used")] public int Used { get; set; }
    [BsonElement("passed")] public int Passed { get; set; }

    public void RegisterUsage(UserModel user, int offset) {
        user.TrainingSets ??= new List<UserTrainSet>();
        var alreadyContains = user.TrainingSets.FirstOrDefault(t => t.SetId == Id);
        if (alreadyContains == null) {
            alreadyContains = new UserTrainSet { SetId = Id };
            user.TrainingSets.Add(alreadyContains);
        }

        alreadyContains.LastSeenWordOffset = offset % Words.Count;
    }
}

[BsonIgnoreExtraElements]
public class WordInLearningSet {
    [BsonElement("w")] public string Word { get; set; }

    [BsonElement("trs")] public string[] AllowedTranslations { get; set; }
    [BsonElement("es")] public ObjectId[] AllowedExamples { get; set; }
}