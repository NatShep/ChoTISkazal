using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SayWhat.MongoDAL.Users {

[BsonIgnoreExtraElements]
public class UserTrainSet {
    [BsonElement("kit")] public ObjectId KitId { get; set; }
<<<<<<< HEAD
    [BsonElement("wc")] public int LastSeenWordOffset { get; set; }
}
=======
    [BsonElement("wc")] public int LastWordChecked { get; set; }
}

>>>>>>> f077f8e (Learning sets db)
}