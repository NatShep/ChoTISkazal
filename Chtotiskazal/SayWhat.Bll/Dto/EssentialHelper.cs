using MongoDB.Bson;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Examples;

namespace SayWhat.Bll.Dto;

public static class EssentialHelper {
    public static EssentialPhrase ToEssentialPhrase(this Example example) {
        var (en, ru) = example.Deconstruct();
        return new(en, ru);
    }

    public static Example ToExample(this EssentialPhrase phrase, EssentialWord word, EssentialTranslation translation) =>
        new()
        {
            Direction = TranslationDirection.EnRu,
            Id = ObjectId.GenerateNewId(),
            OriginPhrase = phrase.En,
            OriginWord = word.En,
            TranslatedWord = translation.Ru,
            TranslatedPhrase = phrase.Ru
        };
}