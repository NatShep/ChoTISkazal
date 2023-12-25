using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SayWhat.MongoDAL.FrequentWords;

[BsonIgnoreExtraElements]
public class FrequentWord
{
    public ObjectId Id { get; set; } = new ObjectId();

    [BsonElement("n")] public int OrderNumber { get; set; }
    [BsonElement("w")] public string Word { get; set; }
    [BsonElement("trs")] public string[] AllowedTranslations { get; set; }
    [BsonElement("es")] public ObjectId[] AllowedExamples { get; set; }
}