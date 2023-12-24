namespace SayWhat.MongoDAL;

// Do not change numbers, as it is used in db
public enum TranslationSource {
    Yadic = 1,
    Manual = 3,
    GoogleTranslate = 4,
    Google2Translate = 5,
    MicrosoftTranslate = 6,
    YandexTranslate = 7,
    BingTranslate = 8,
    UnknownGTranslate = 9,
    /// <summary>
    /// Automatically added from the Add mutual phrase job
    /// </summary>
    AutoPhrase = 100,
    Restored = 200,
    
}