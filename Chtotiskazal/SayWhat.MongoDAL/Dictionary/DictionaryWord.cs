using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SayWhat.MongoDAL.Dictionary
{
    public class DictionaryWord {
        // ReSharper disable once InconsistentNaming
        public ObjectId  Id { get; set; }
        [BsonElement(DictionaryRepo.WordFieldBsonName)]
        public string Word { get; set; }
        [BsonElement("ts")]
        public string Transcription { get; set; }
        [BsonElement("l")]
        public Language Language { get; set; }
        [BsonElement("tr")]
        public DictionaryTranslation[] Translations { get; set; }
        [BsonElement("src")]
        public TranslationSource Source { get; set; }
    }
}