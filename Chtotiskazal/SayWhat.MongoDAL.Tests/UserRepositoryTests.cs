using System.Threading.Tasks;
using NUnit.Framework;
using SayWa.MongoDAL.Users;

namespace SayWa.MongoDAL.Tests
{
    public class UserRepositoryTests
    {
        private UsersRepository _repository;
        
        [SetUp]
        public void Intitalize()
        {
            MongoTestHelper.DropAllCollections();
            _repository =new UsersRepository(MongoTestHelper.Database);
            _repository.UpdateDb().Wait();
        }
        [Test]
        public async Task Add_GetReturnsInt()
        {
            await _repository.AddFromTelegram(1234567, "vasa");
            var user = await _repository.GetOrDefaultByTelegramId(1234567);
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
                _repository.AddFromTelegram(i, "vasa").Wait();
            }
            Assert.AreEqual(count, _repository.GetCount().Result);
        }

        [Test]
        public void AddSameTelegramId_Throws()
        {
            _repository.AddFromTelegram(123, "x").Wait();
            Assert.Catch(() => _repository.AddFromTelegram(123, "y").Wait());
        }
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(42)]
        public void AddSeveralUsersWithoutTelegramId_DoesNotThrows(int count)
        {
            for (int i = 0; i < count; i++)
            {
                _repository.Add(new User{TelegramId =  null, Nick = $"petr{count}"}).Wait();
            }
            Assert.AreEqual(count, _repository.GetCount().Result);
        }

    }
}