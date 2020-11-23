using System.Threading.Tasks;
using NUnit.Framework;
using SayWhat.MongoDAL.Users;

namespace SayWhat.MongoDAL.Tests
{
    public class UserRepoTests
    {
        private UsersRepo _repo;
        
        [SetUp]
        public void Intitalize()
        {
            MongoTestHelper.DropAllCollections();
            _repo =new UsersRepo(MongoTestHelper.Database);
            _repo.UpdateDb().Wait();
        }
        [Test]
        public async Task Add_GetReturnsInt()
        {
            await _repo.AddFromTelegram(1234567, "vasa");
            var user = await _repo.GetOrDefaultByTelegramId(1234567);
            Assert.AreEqual(1234567, user.TelegramId);
            Assert.AreEqual("vasa", user.Nick);
            Assert.AreEqual(UserSource.Telegram, user.Source);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(42)]
        public void Add_GetCountReturnsSize(int count)
        {
            for (int i = 0; i < count; i++)
            {
                _repo.AddFromTelegram(i, "vasa").Wait();
            }
            Assert.AreEqual(count, _repo.GetCount().Result);
        }

        [Test]
        public void AddSameTelegramId_Throws()
        {
            _repo.AddFromTelegram(123, "x").Wait();
            Assert.Catch(() => _repo.AddFromTelegram(123, "y").Wait());
        }
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(42)]
        public void AddSeveralUsersWithoutTelegramId_DoesNotThrows(int count)
        {
            for (int i = 0; i < count; i++)
            {
                _repo.Add(new User{TelegramId =  null, Nick = $"petr{count}"}).Wait();
            }
            Assert.AreEqual(count, _repo.GetCount().Result);
        }

    }
}