using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using NUnit.Framework;
using SayWhat.MongoDAL.Users;
using SayWhat.MongoDAL.Words;

namespace SayWhat.MongoDAL.Tests
{
    public class UserWordsRepoTests
    {
        private UserWordsRepo _repo;
        
        [SetUp]
        public void Intitalize()
        {
            MongoTestHelper.DropAllCollections();
            _repo =new UserWordsRepo(MongoTestHelper.Database);
            _repo.UpdateDb().Wait();
        }
        [Test]
        public async Task Add_GetAllForUserReturnsIt()
        {
            var user = new User {Id = ObjectId.GenerateNewId()};
            await _repo.Add(CreateWord(user.Id,"table", "стол" ));
            var allWords =  await _repo.GetAllUserWordsAsync(user);
            Assert.AreEqual(1, allWords.Count);
            Assert.AreEqual("table", allWords[0].Word);
            Assert.AreEqual("стол", allWords[0].Translations[0].Translation);
        }

        [TestCase(0,0)]
        [TestCase(0,1)]
        [TestCase(0,10)]
        [TestCase(1,0)]
        [TestCase(1,1)]
        [TestCase(1,10)]
        [TestCase(10,0)]
        [TestCase(10,1)]
        [TestCase(10,10)]
        public async Task AddSeveral_GetWorstReturnWorstOnes(int worstCount, int bestCount)
        {
            var user = new User {Id = ObjectId.GenerateNewId()};
            var worstOnes = new List<UserWord>();
            for (int i = 0; i < worstCount; i++)
            {
                var word = CreateWord(user.Id, $"table{i}", $"стол{i}", i);
                worstOnes.Add(word);
            }
            var rand = new Random();
            var randomList = worstOnes.OrderBy (x => rand.Next()).ToList();
            
            foreach (var word in randomList)
            {
                await _repo.Add(word);
            }

            for (int i = 0; i < bestCount; i++)
                await _repo.Add(CreateWord(user.Id,$"table{i}", $"стол{i}", i+worstCount ));

            var allWords =  await _repo.GetWorstLearned(user,worstCount);
            
            for (int i = 0; i < worstCount; i++)
            {
                var origin = worstOnes[i];
                var current = allWords[i];
                Assert.AreEqual(origin.Word, current.Word);
                Assert.AreEqual(origin.Translations[0].Translation, current.Translations[0].Translation);
            }
        }
        
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(42)]
        public async Task AddSeveral_GetAllForUserReturnsThem(int count)
        {
            var user = new User {Id = ObjectId.GenerateNewId()};
            for (int i = 0; i < count; i++) 
                await _repo.Add(CreateWord(user.Id,$"table{i}", "стол{i}" ));
            
            var allWords =  await _repo.GetAllUserWordsAsync(user);
            Assert.AreEqual(count, allWords.Count);
        }

        private static UserWord CreateWord(ObjectId userId, string word, string tranlation,double rate = 0 )
        {
            return new UserWord{
                UserId = userId, 
                Word = word, 
                CurrentRate = rate,
                Translations = new[]
            {
                new UserWordTranslation
                {
                    Translation = tranlation
                }
            }};
        }
    }
}