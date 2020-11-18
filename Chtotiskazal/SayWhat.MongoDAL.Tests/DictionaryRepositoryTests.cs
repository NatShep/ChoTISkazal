using System;
using MongoDB.Bson;
using NUnit.Framework;
using SayWa.MongoDAL.Dictionary;

namespace SayWa.MongoDAL.Tests
{
    [TestFixture]
    public class DictionaryRepositoryTests
    {
        private DictionaryRepository _repository;
        
        [SetUp]
        public void Intitalize()
        {
            MongoTestHelper.DropAllCollections();
            _repository =new DictionaryRepository(MongoTestHelper.Database);
            _repository.UpdateDb().Wait();
        }
        [Test]
        public void AddWord_WordExistsInCollection()
        {
           var word = CreateWord("table", "cтол");
           
           _repository.Add(word).Wait();
           var read =  _repository.GetOrDefault("table").Result;
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
                _repository.Add(CreateWord($"table{i}", "cтол")).Wait();
            }
            Assert.AreEqual(count, _repository.GetCount().Result);
        }

        [Test]
        public void AddSameWord_Throws()
        {
            _repository.Add(CreateWord("table", "cтол")).Wait();
            Assert.Catch(()=> _repository.Add(CreateWord("table", "таблица")).Wait());
        }

        private DictionaryWord CreateWord(string origin, string translation)
        {
            return new DictionaryWord
            {
                Id = ObjectId.GenerateNewId(),
                Language = Language.Ru,
                Word = origin,
                Transcription = "Stol",
                Translations = new[]
                {
                    new DictionaryTranslation
                    {
                        Id = ObjectId.GenerateNewId(),
                        Language = Language.En,
                        Word = translation,
                        Transcription = "teibl",
                        Examples = new[]
                        {
                            new DictionaryExample
                            {
                                Id = ObjectId.GenerateNewId(),
                                OriginExample = $"Что это за {origin}?",
                                TranslationExample = $"What the {translation}?"
                            }
                        }
                    }
                }
            };
        }
    }
}