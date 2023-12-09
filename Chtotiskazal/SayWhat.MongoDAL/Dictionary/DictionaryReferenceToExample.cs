using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SayWhat.MongoDAL.Examples;

namespace SayWhat.MongoDAL.Dictionary;

public class DictionaryReferenceToExample {
    public DictionaryReferenceToExample() { }

    public DictionaryReferenceToExample(Example example) {
        ExampleId = example.Id;
        ExampleOrNull = example;
    }

    [BsonElement("eid")] public ObjectId ExampleId { get; set; }
    [BsonIgnore] public Example ExampleOrNull { get; private set; }

    public bool TryLoadExample(Dictionary<ObjectId, Example> examples) {
        ExampleOrNull = examples.GetValueOrDefault(ExampleId);
        return ExampleOrNull != null;
    }
}