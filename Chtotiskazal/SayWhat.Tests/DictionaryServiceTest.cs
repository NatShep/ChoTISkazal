using System.Threading.Tasks;
using MongoDB.Bson;
using NUnit.Framework;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Dictionary;
using SayWhat.MongoDAL.Examples;

namespace SayWhat.MongoDAL.Tests
{
    public class DictionaryServiceTest
    {
        [Test]
        public async Task AddWordToDictionary_GetWithExampleReturnsSame()
        {
            MongoTestHelper.DropAllCollections();
            var dictionaryRepo = new DictionaryRepo(MongoTestHelper.Database);
            var examplesRepo = new ExamplesRepo(MongoTestHelper.Database);
            var service = new DictionaryService(dictionaryRepo, examplesRepo);
            var example = new Example
            {
                Direction = TranlationDirection.EnRu,
                Id = ObjectId.GenerateNewId(),
                OriginWord = "table",
                TranslatedWord = "стол",
                OriginPhrase = "What the table?",
                TranslatedPhrase = "Какого стола?" 
            };
            var word = new DictionaryWord
            {
                Id  = ObjectId.GenerateNewId(),
                Word = "table",
                Language = Language.En,
                Source = TranslationSource.Yadic,
                Transcription = "qweqwe",
                Translations = new[]
                {
                    new DictionaryTranslation
                    {
                        Word = "Стол",
                        Language = Language.Ru,
                        Examples = new []
                        { 
                            new DictionaryReferenceToExample()
                            { 
                                ExampleId = example.Id,
                                ExampleOrNull =example,
                            }
                        }
                    }
                }
                
            };
            await service.AddNewWord(word);
            var translations = await service.GetTranslationsWithExamples("table");
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
    }
}