using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SayWhat.MongoDAL.Examples;

namespace SayWhat.MongoDAL.Words
{
    public class UserWordTranslationReferenceToExample {
        public UserWordTranslationReferenceToExample()
        {
            
        }

        public UserWordTranslationReferenceToExample(ObjectId exampleId)
        {
            ExampleId = exampleId;
        }

        public UserWordTranslationReferenceToExample(Example example)
        {
            ExampleId = example.Id;
            ExampleOrNull = example;
        }
        public ObjectId ExampleId { get; set; }
        [BsonIgnore] 
        public Example ExampleOrNull { get; set; }
    }
}