using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SayWhat.MongoDAL.Users {

[BsonIgnoreExtraElements]
public class UserTrainSet {
    [BsonElement("set")] public ObjectId SetId { get; set; }
    [BsonElement("wc")] public int LastSeenWordOffset { get; set; }
}

}