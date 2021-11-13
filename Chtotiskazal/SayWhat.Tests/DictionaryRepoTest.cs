using MongoDB.Bson;
using NUnit.Framework;
using SayWhat.MongoDAL.Dictionary;

namespace SayWhat.MongoDAL.Tests
{
    [TestFixture]
    public class DictionaryRepoTests
    {
        private LocalDictionaryRepo _repo;
        
        [SetUp]
        public void Intitalize()
        {
            MongoTestHelper.DropAllCollections();
            _repo =new LocalDictionaryRepo(MongoTestHelper.Database);
            _repo.UpdateDb().Wait();
        }
        [Test]
        public void AddWord_WordExistsInCollection()
        {
           var word = CreateWord("table", "cтол");
           
           _repo.Add(word).Wait();
           var read =  _repo.GetOrDefault("table").Result;
           Assert.AreEqual(read.Transcription, word.Transcription);
           Assert.AreEqual(read.Word, word.Word);
           Assert.AreEqual(read.Translations.Length, word.Translations.Length);
           Assert.AreEqual(read.Translations[0].Word, word.Translations[0].Word);
           Assert.AreEqual(read.Translations[0].Examples.Length, word.Translations[0].Examples.Length);
        }


        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(42)]
        public void AddWord_GetCountReturnsSize(int count)
        {
            for (int i = 0; i < count; i++)
            {
                _repo.Add(CreateWord($"table{i}", "cтол")).Wait();
            }
            Assert.AreEqual(count, _repo.GetCount().Result);
        }

        [Test]
        public void AddSameWord_Throws()
        {
            _repo.Add(CreateWord("table", "cтол")).Wait();
            Assert.Catch(()=> _repo.Add(CreateWord("table", "таблица")).Wait());
        }

        private DictionaryWord CreateWord(string origin, string translation) =>
            new DictionaryWord
            {
                Id = ObjectId.GenerateNewId(),
                Language = Language.Ru,
                Word = origin,
                Transcription = "Stol",
                Translations = new[]
                {
                    new DictionaryTranslation
                    {
                        Language = Language.En,
                        Word = translation,
                        Examples = new[]
                        {
                            new DictionaryReferenceToExample
                            {
                                ExampleId = ObjectId.GenerateNewId()
                            }
                        }
                    }
                }
            };
    }
}