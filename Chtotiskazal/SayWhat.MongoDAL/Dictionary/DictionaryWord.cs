using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SayWhat.MongoDAL.Dictionary
{
    public class DictionaryWord {
        public ObjectId  Id { get; set; }
        [BsonElement(DictionaryRepo.WordFieldBsonName)]
        public string Word { get; set; }
        public string Transcription { get; set; }
        public Language Language { get; set; }
        public DictionaryTranslation[] Translations { get; set; }
        public TranslationSource Source { get; set; }
    }
}