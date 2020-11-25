using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using NUnit.Framework;
using SayWhat.MongoDAL.Examples;

namespace SayWhat.MongoDAL.Tests
{
    public class ExampleRepoTests
    {
        private ExamplesRepo _repo;
        
        [SetUp]
        public void Intitalize()
        {
            MongoTestHelper.DropAllCollections();
            _repo =new ExamplesRepo(MongoTestHelper.Database);
            _repo.UpdateDb().Wait();
        }
        [Test]
        public async Task AddOne_GetReturnsIt()
        {
            var example = CreateExample("table","стол");
            await _repo.Add(example);
            var read = await _repo.GetOrDefault(example._id);
            
            Assert.AreEqual(example.OriginPhrase, read.OriginPhrase);
            Assert.AreEqual(example.OriginWord, read.OriginWord);
            Assert.AreEqual(example.TranslatedPhrase, read.TranslatedPhrase);
            Assert.AreEqual(example.TranslatedWord, read.TranslatedWord);
        }

        [Test]
        public async Task AddOne_GetAllReturnsIt()
        {
            var example = CreateExample("table","стол");
            await _repo.Add(example);
            var all = await _repo.GetAll(new[]{example._id});
            Assert.AreEqual(1,all.Count);
            var read = all.First();
            Assert.AreEqual(example.OriginPhrase, read.OriginPhrase);
            Assert.AreEqual(example.OriginWord, read.OriginWord);
            Assert.AreEqual(example.TranslatedPhrase, read.TranslatedPhrase);
            Assert.AreEqual(example.TranslatedWord, read.TranslatedWord);
        }
        
        [Test]
        public async Task Empty_GetAllReturnsEmptyList()
        {
            var all = await _repo.GetAll(new[]{ObjectId.GenerateNewId()});
            Assert.AreEqual(0,all.Count);
        }
        
        [Test]
        public async Task AddOne_GetDifferentReturnsNull()
        {
            var example = CreateExample("table","стол");
            await _repo.Add(example);
            var read = await _repo.GetOrDefault(ObjectId.GenerateNewId());
            Assert.IsNull(read);
        }

        private Example CreateExample(string originWord, string translatedWord)
            =>
                new Example
                {
                    _id = ObjectId.GenerateNewId(),
                    OriginPhrase = "on the table", OriginWord = originWord, TranslatedPhrase = "на столе",
                    TranslatedWord = translatedWord
                };
       

    }
}