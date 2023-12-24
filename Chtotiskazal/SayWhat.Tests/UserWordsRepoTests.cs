using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using NUnit.Framework;
using SayWhat.MongoDAL.Users;
using SayWhat.MongoDAL.Words;

namespace SayWhat.MongoDAL.Tests;

public class UserWordsRepoTests {
    private UserWordsRepo _repo;

    [SetUp]
    public void Initialize() {
        MongoTestHelper.DropAllCollections();
        _repo = new UserWordsRepo(MongoTestHelper.Database);
        _repo.UpdateDb().Wait();
    }

    [Test]
    public async Task Add_GetAllForUserReturnsIt() {
        var user = new UserModel { Id = ObjectId.GenerateNewId() };
        await _repo.Add(new UserWordModel(user.Id, "table", "стол", UserWordType.UsualWord, 0));
        var allWords = await _repo.GetAllUserWordsAsync(user);
        Assert.AreEqual(1, allWords.Count);
        Assert.AreEqual("table", allWords[0].Word);
        Assert.AreEqual("стол", allWords[0].RuTranslations[0].Word);
    }

    [TestCase(0, 0)]
    [TestCase(0, 1)]
    [TestCase(0, 10)]
    [TestCase(1, 0)]
    [TestCase(1, 1)]
    [TestCase(1, 10)]
    [TestCase(10, 0)]
    [TestCase(10, 1)]
    [TestCase(10, 10)]
    public async Task AddSeveral_GetWorstReturnWorstOnes(int worstCount, int bestCount) {
        #region arrange

        var user = new UserModel { Id = ObjectId.GenerateNewId() };
        var worstOnes = new List<UserWordModel>();
        for (int i = 0; i < worstCount; i++) {
            string word1 = $"table{i}";
            string tranlation = $"стол{i}";
            var word = new UserWordModel(user.Id, word1, tranlation, UserWordType.UsualWord, TranslationSource.Manual, i);
            worstOnes.Add(word);
        }

        var randomList = worstOnes.OrderBy(x => Rand.Next()).ToList();

        foreach (var word in randomList) {
            await _repo.Add(word);
        }

        for (int i = 0; i < bestCount; i++) {
            string word = $"table{i}";
            string tranlation = $"стол{i}";
            double rate = i + worstCount;
            await _repo.Add(new UserWordModel(user.Id, word, tranlation, UserWordType.UsualWord, TranslationSource.Manual, rate));
        }

        #endregion

        //TODO 

        #region act

        #endregion

        //TODO 

        #region assert

        /*
         for (int i = 0; i < worstCount; i++)
        {
            var origin = worstOnes[i];
            var current = allWords[i];
            Assert.AreEqual(origin.Word, current.Word);
            Assert.AreEqual(origin.RuTranslations[0].Word, current.RuTranslations[0].Word);
        }
        */

        #endregion
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(42)]
    public async Task AddSeveral_GetAllForUserReturnsThem(int count) {
        var user = new UserModel { Id = ObjectId.GenerateNewId() };
        for (int i = 0; i < count; i++) {
            string word = $"table{i}";
            await _repo.Add(new UserWordModel(user.Id, word, "стол{i}", UserWordType.UsualWord, 0));
        }

        var allWords = await _repo.GetAllUserWordsAsync(user);
        Assert.AreEqual(count, allWords.Count);
    }

    [Test]
    public async Task TableHasNoWordsForUser_HasAnyReturnsFalse() {
        var user = new UserModel { Id = ObjectId.GenerateNewId() };
        await _repo.Add(new UserWordModel(user.Id, "table", "стол", UserWordType.UsualWord, 0));
        var hasAny = await _repo.HasAnyFor(new UserModel { Id = ObjectId.GenerateNewId() });
        Assert.IsFalse(hasAny);
    }

    [Test]
    public async Task TableHasWordsForUser_HasAnyReturnsTrue() {
        var user = new UserModel { Id = ObjectId.GenerateNewId() };
        await _repo.Add(new UserWordModel(user.Id, "table", "стол", UserWordType.UsualWord, 0));
        var hasAny = await _repo.HasAnyFor(user);
        Assert.True(hasAny);
    }

    [Test]
    public async Task TableHasWordForUser_GetWordReturnIt() {
        var user = new UserModel { Id = ObjectId.GenerateNewId() };
        await _repo.Add(new UserWordModel(user.Id, "table", "стол", UserWordType.UsualWord, 0));
        var word = await _repo.GetWordOrDefault(user, "table");
        Assert.IsNotNull(word);
        Assert.AreEqual("table", word.Word);
        Assert.AreEqual("стол", word.RuTranslations[0].Word);
    }

    [Test]
    public async Task TableHasWordForOtherUser_GetWordReturnNull() {
        var user = new UserModel { Id = ObjectId.GenerateNewId() };
        await _repo.Add(new UserWordModel(user.Id, "table", "стол", UserWordType.UsualWord, 0));
        var word = await _repo.GetWordOrDefault(new UserModel { Id = ObjectId.GenerateNewId() }, "table");
        Assert.IsNull(word);
    }

    [Test]
    public async Task AddTranslations_Update_GetReturnsUpdated() {
        var user = new UserModel { Id = ObjectId.GenerateNewId() };
        var word = new UserWordModel(user.Id, "table", "стол", UserWordType.UsualWord, TranslationSource.Yadic);
        await _repo.Add(word);
        word.AddTranslations(
            new List<UserWordTranslation>
            {
                new UserWordTranslation
                {
                    Word = "таблица",
                    Examples = new[]
                    {
                        new UserWordTranslationReferenceToExample()
                        {
                            ExampleId = ObjectId.GenerateNewId()
                        }
                    }
                }
            });
        await _repo.Update(word);
        var readWord = await _repo.GetWordOrDefault(user, "table");
        Assert.IsNotNull(readWord);
        Assert.AreEqual(2, readWord.RuTranslations.Length);
        Assert.AreEqual("стол", readWord.RuTranslations[1].Word);
        Assert.AreEqual("таблица", readWord.RuTranslations[0].Word);
    }
}