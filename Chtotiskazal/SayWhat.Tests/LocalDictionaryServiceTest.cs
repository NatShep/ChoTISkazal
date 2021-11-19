using System.Threading.Tasks;
using MongoDB.Bson;
using NUnit.Framework;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Dictionary;
using SayWhat.MongoDAL.Examples;

namespace SayWhat.MongoDAL.Tests {

public class LocalDictionaryServiceTest {
    private LocalDictionaryRepo _dictionaryRepo;
    private ExamplesRepo _examplesRepo;
    private LocalDictionaryService _service;

    [SetUp]
    public void Init() {
        MongoTestHelper.DropAllCollections();
        _dictionaryRepo = new LocalDictionaryRepo(MongoTestHelper.Database);
        _examplesRepo = new ExamplesRepo(MongoTestHelper.Database);
        _service = new LocalDictionaryService(_dictionaryRepo, _examplesRepo);
    }

    [Test]
    public async Task AddWordToDictionary_GetAllTranslationWords() {
        await _service.AddNewWord(CreateWord("rate", "", ("коэффициент", new Example[0])));
        await _service.AddNewWord(CreateWord("coefficient", "", ("коэффициент", new Example[0])));

        var rus = await _service.GetAllTranslationWords("rate".ToLower());
        Assert.AreEqual(1, rus.Length);
        Assert.AreEqual("коэффициент", rus[0]);
    }

    /**
     * // ## Other translation case ##
            
            // example: 
            //     Coefficient - коэффициент
            //     Rate        - коэффициент
            // Question is about 'коэффициент' (Coefficient)
            // User answers 'Rate'
            // Search for 'Rate' translations
            var otherRuTranslationsOfUserInput = await _localDictionaryService.GetAllTranslationWords(enUserEntry.ToLower());
     */
    [Test]
    public async Task AddWordToDictionary_GetWithExampleReturnsSame() {
        var example = new Example {
            Direction = TranslationDirection.EnRu,
            Id = ObjectId.GenerateNewId(),
            OriginWord = "table",
            TranslatedWord = "стол",
            OriginPhrase = "What the table?",
            TranslatedPhrase = "Какого стола?"
        };
        
        var word = CreateWord("table", "qweqwe", ("Стол", new[] { example }));
        await _service.AddNewWord(word);
        var translations = await _service.GetTranslationsWithExamples("table");
        Assert.AreEqual(1, translations.Count);
        var translation = translations[0];
        Assert.AreEqual(word.Source, translation.Source);
        Assert.AreEqual(word.Transcription, translation.EnTranscription);
        Assert.AreEqual(word.Word, translation.OriginText);
        Assert.AreEqual(word.Translations[0].Word, translation.TranslatedText);

        Assert.AreEqual(example.OriginWord, translation.Examples[0].OriginWord);
        Assert.AreEqual(example.OriginPhrase, translation.Examples[0].OriginPhrase);
        Assert.AreEqual(example.TranslatedWord, translation.Examples[0].TranslatedWord);
        Assert.AreEqual(example.TranslatedPhrase, translation.Examples[0].TranslatedPhrase);
    }

    public static DictionaryWord CreateWord(string en, string transcription, params (string, Example[])[] translations) {
        return  new DictionaryWord {
            Id = ObjectId.GenerateNewId(),
            Word = en,
            Language = Language.En,
            Source = TranslationSource.Yadic,
            Transcription = transcription,
            Translations = translations.SelectToArray(t=> new DictionaryTranslation {
                Word = t.Item1,
                Language = Language.Ru,
                Examples = t.Item2.SelectToArray(i=>new DictionaryReferenceToExample(i))
            })
        };
    }
}


}