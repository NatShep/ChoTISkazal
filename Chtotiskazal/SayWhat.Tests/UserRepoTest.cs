using System.Threading.Tasks;
using NUnit.Framework;
using SayWhat.MongoDAL.Users;
using SayWhat.MongoDAL.Words;

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
        public async Task Add_GetReturnsIt()
        {
            await _repo.AddFromTelegram(1234567, "vasa","popov","vasa97");
            var user = await _repo.GetOrDefaultByTelegramIdOrNull(1234567);
            Assert.AreEqual(1234567, user.TelegramId);
            Assert.AreEqual("vasa97", user.TelegramNick);
            Assert.AreEqual("vasa", user.TelegramFirstName);
            Assert.AreEqual("popov", user.TelegramLastName);
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
                _repo.AddFromTelegram(i, "vasa","","").Wait();
            }
            Assert.AreEqual(count, _repo.GetCount().Result);
        }

        [Test]
        public void AddSameTelegramId_Throws()
        {
            _repo.AddFromTelegram(123, "x","","").Wait();
            Assert.Catch(() => _repo.AddFromTelegram(123, "y","","").Wait());
        }
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(42)]
        public void AddSeveralUsersWithoutTelegramId_DoesNotThrows(int count)
        {
            for (int i = 0; i < count; i++)
            {
                _repo.Add(new UserModel(
                    telegramId: null,
                    lastName: "",
                    firstName:"",
                    telegramNick: $"petr{count}",
                    source: UserSource.Telegram)).Wait();
            }
            Assert.AreEqual(count, _repo.GetCount().Result);
        }

        [TestCase(1)]
        [TestCase(42)]
        public async Task OnLearningDone_UpdatedModelContainsMothlyAndDailyStats(int updateCount)
        {
            var model = await _repo.AddFromTelegram(1234567, "vasa", "popov", "vasa97");
            for (int i = 0; i < updateCount; i++)
                model.OnLearningDone();
            
            await _repo.Update(model);
            var read = await _repo.GetOrDefaultByTelegramIdOrNull(1234567);
            Assert.AreEqual(1,read.LastDaysStats.Count);
            Assert.AreEqual(1,read.LastMonthStats.Count);
        }
        
        [TestCase(1)]
        [TestCase(42)]
        public void ModelOnLearningDone_ModelContainsMothlyAndDailyStats(int updateCount)
        {
            var model = new UserModel(1234567, "vasa", "popov", "vasa97", UserSource.Telegram);
            for (int i = 0; i < updateCount; i++)
                model.OnLearningDone();
            
            Assert.AreEqual(1, model.LastDaysStats.Count);
            Assert.AreEqual(1, model.LastMonthStats.Count);
        }
        
        [TestCase(1)]
        [TestCase(42)]
        public async Task OnPairsAdded_UpdatedModelContainsMothlyAndDailyStats(int updateCount)
        {
            var model = await _repo.AddFromTelegram(1234567, "vasa", "popov", "vasa97");
            for (int i = 0; i < updateCount; i++)
                model.OnPairsAdded(WordStatsChanging.Zero,  1,1);
            await _repo.Update(model);

            var read = await _repo.GetOrDefaultByTelegramIdOrNull(1234567);
            Assert.AreEqual(1,read.LastDaysStats.Count);
            Assert.AreEqual(1,read.LastMonthStats.Count);
        }
        
        [TestCase(1)]
        [TestCase(42)]
        public async Task OnQuestionPassed_UpdatedModelContainsMothlyAndDailyStats(int updateCount)
        {
            var model = await _repo.AddFromTelegram(1234567, "vasa", "popov", "vasa97");
            for (int i = 0; i < updateCount; i++)
                model.OnQuestionPassed(WordStatsChanging.Zero);
            await _repo.Update(model);

            var read = await _repo.GetOrDefaultByTelegramIdOrNull(1234567);
            Assert.AreEqual(1,read.LastDaysStats.Count);
            Assert.AreEqual(1,read.LastMonthStats.Count);
        }
        
        [TestCase(1)]
        [TestCase(42)]
        public async Task OnQuestionFailed_UpdatedModelContainsMothlyAndDailyStats(int updateCount)
        {
            var model = await _repo.AddFromTelegram(1234567, "vasa", "popov", "vasa97");
            for (int i = 0; i < updateCount; i++)
                model.OnQuestionFailed(WordStatsChanging.Zero);
            await _repo.Update(model);

            var read = await _repo.GetOrDefaultByTelegramIdOrNull(1234567);
            Assert.AreEqual(1,read.LastDaysStats.Count);
            Assert.AreEqual(1,read.LastMonthStats.Count);
        }

        [Test]
        public async Task Update_GetReturnsUpdated()
        {
            var model = await _repo.AddFromTelegram(1234567, "vasa","popov","vasa97");
            model.OnLearningDone();
            model.OnLearningDone();
            model.OnLearningDone();
            
            model.OnNewWordAdded(WordStatsChanging.CreateForNewWord(0) , 24,124);
            model.OnEnglishWordTranslationRequest();
            model.OnEnglishWordTranslationRequest();
            model.OnEnglishWordTranslationRequest();
            model.OnRussianWordTranlationRequest();

            await _repo.Update(model);
            var user = await _repo.GetOrDefaultByTelegramIdOrNull(1234567);

            Assert.AreEqual(1234567, user.TelegramId);
            Assert.AreEqual("vasa97", user.TelegramNick);
            Assert.AreEqual("vasa", user.TelegramFirstName);
            Assert.AreEqual("popov", user.TelegramLastName);
            Assert.AreEqual(UserSource.Telegram, user.Source);
            Assert.AreEqual(3, user.EnglishWordTranslationRequestsCount);
            Assert.AreEqual(1, user.RussianWordTranslationRequestsCount);
            Assert.AreEqual(24, user.PairsCount);
            Assert.AreEqual(124, user.ExamplesCount);
        }

    }
}