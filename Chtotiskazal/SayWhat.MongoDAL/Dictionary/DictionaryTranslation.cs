using MongoDB.Bson;

namespace SayWhat.MongoDAL.Dictionary
{
    public class DictionaryTranslation {
        public ObjectId Id { get; set; }
        public string   Word    { get; set; }
        public Language Language    { get; set; }
        public DictionaryReferenceToExample[] Examples  { get; set; }
    }
}