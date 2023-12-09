using System;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SayWhat.MongoDAL.Examples;

public class Example
{
    public Example()
    {
        Direction = TranslationDirection.EnRu;
    }
    [BsonIgnore]
    public string[] SplitWordsOfPhrase => OriginPhrase.Split(" ", StringSplitOptions.RemoveEmptyEntries);
    public ObjectId Id { get; set; }
    [BsonElement("to")]
    public string OriginWord { get; set; }
    [BsonElement("tw")]
    public string TranslatedWord { get; set; }
    [BsonElement("op")]
    public string OriginPhrase { get; set; }
    [BsonElement("tp")]
    public string TranslatedPhrase { get; set; }
    [BsonElement("lng")]
    public TranslationDirection Direction { get; set; }
        
    public (string en, string ru) Deconstruct() {
        var en = Direction == TranslationDirection.EnRu ? OriginPhrase : TranslatedPhrase;
        var ru = Direction == TranslationDirection.EnRu ? TranslatedPhrase : OriginPhrase;
        return (en, ru);
    }
}