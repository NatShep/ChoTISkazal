using System;
using System.IO;
using Chotiskazal.Logic.DAL;
using Dic.Logic.DAL;
using NUnit.Framework;

namespace Chotiskazal.Tests
{
    public class WordRepositoryTest
    {
        [SetUp]
        public void Setup()
        {
            var repo = new WordsRepository("test");
            File.Delete(repo.DbFile);
        }

        [Test]
        public void ApplyMigrations()
        {
            var repo =  new WordsRepository("test");
            repo.ApplyMigrations();

        }
        [Test]
        public void GetOrNullPair_ReturnsPhrase()
        {
            var repo = new WordsRepository("test");
            repo.ApplyMigrations();
         
            var word = repo.GetOrNullWithPhrases("word");
            Assert.IsNotNull(word.Phrases);
            Assert.AreEqual(1, word.Phrases.Count);
            Assert.AreEqual("what is word", word.Phrases[0].Origin);
        }
        [Test]
        public void GetWorst10_ReturnsPhrase()
        {
            var repo = new WordsRepository("test");
            repo.ApplyMigrations();
            repo.CreateNew("word", "translation", "trans", new[]
            {
                new Phrase()
                {
                    Created = DateTime.Now,
                    Origin = "what is word",
                    OriginWord = "word",
                    Translation = "trans is trans",
                    TranslationWord = "trans"
                },
                new Phrase()
                {
                    Created = DateTime.Now,
                    Origin = "what is word 2",
                    OriginWord = "word",
                    Translation = "trans is trans 2",
                    TranslationWord = "trans"
                },
            });
            repo.CreateNew("word2", "translation2", "trans", new[]
            {
                new Phrase()
                {
                    Created = DateTime.Now,
                    Origin = "what is word",
                    OriginWord = "word2",
                    Translation = "trans is trans",
                    TranslationWord = "trans2"
                },
                new Phrase()
                {
                    Created = DateTime.Now,
                    Origin = "what is word 2",
                    OriginWord = "word2",
                    Translation = "trans is trans 2",
                    TranslationWord = "trans2"
                },
            });
            repo.CreateNew("word3", "translation3", "trans", new[]
            {
                new Dic.Logic.DAL.Phrase()
                {
                    Created = DateTime.Now,
                    Origin = "what is word",
                    OriginWord = "word3",
                    Translation = "trans is trans",
                    TranslationWord = "trans3"
                },
                new Phrase()
                {
                    Created = DateTime.Now,
                    Origin = "what is word 3",
                    OriginWord = "word3",
                    Translation = "trans is trans 3",
                    TranslationWord = "trans3"
                },
            });
            var words = repo.GetWorst(2);
            Assert.IsNotNull(words);
            Assert.AreEqual(2, words.Length);
        }
    }
}