using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SayWa.MongoDAL.Dictionary
{
    public class DictionaryWord {
        public ObjectId  Id { get; set; }
        [BsonElement(DictionaryRepository.WordFieldBsonName)]
        public string Word { get; set; }
        public string Transcription { get; set; }
        public Language Language { get; set; }
        public DictionaryTranslation[] Translations { get; set; }
    }
}