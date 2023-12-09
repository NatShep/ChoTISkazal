using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SayWhat.MongoDAL.Words;

namespace SayWhat.MongoDAL.LongDataForTranslationButton;

[BsonIgnoreExtraElements]
public class LongCallbackData {
    public LongCallbackData(string word, string translation) {
        Id = ObjectId.GenerateNewId();
        _word = word;
        _translation = translation;
    }

    #region mongo fields

    public ObjectId Id { get; set; }

    [BsonElement(UserWordsRepo.OriginWordFieldName)]
    private string _word;

    [BsonElement("tr")] private string _translation;

    #endregion

    public string Word => _word;
    public string Translation => _translation;
}