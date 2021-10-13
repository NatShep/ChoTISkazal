using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using NUnit.Framework;
using SayWhat.MongoDAL.WordKits;

namespace SayWhat.MongoDAL.Tests {

public class LearningSetsRepoTests {
    private LearningSetsRepo _repo;

    [SetUp]
    public void Initialize() {
        MongoTestHelper.DropAllCollections();
        _repo = new LearningSetsRepo(MongoTestHelper.Database);
        _repo.UpdateDb().Wait();
    }

    [Test]
    public async Task GetAll_EmptyRepo_returnsEmptyCollection() {
        var sets = await _repo.GetAll();
        Assert.IsEmpty(sets);
    }

    [Test]
    public async Task GetAll_SingleItemRepo_returnsIt() {
        var model = CreateLearningSetModel();
        await _repo.Add(model);
        var sets = await _repo.GetAll();
        Assert.AreEqual(1, sets.Count);
        AssertModelsAreEqual(model, sets[0]);
    }


    [Test]
    public async Task Get_NonExist_returnsNull() {
        var model = CreateLearningSetModel();
        await _repo.Add(model);
        var item = await _repo.GetOrDefault(new ObjectId());
        Assert.IsNull(item);
    }

    [Test]
    public async Task Get_Exists_returnsIt() {
        var model = CreateLearningSetModel();
        await _repo.Add(model);
        var item = await _repo.GetOrDefault(model.Id);
        AssertModelsAreEqual(model, item);
    }

    [Test]
    public async Task GetCount_EmptyRepo_returnsZero() {
        var count = await _repo.GetCount();
        Assert.AreEqual(0, count);
    }

    [Test]
    public async Task GetCount_nonEmptyRepo_returnsCount() {
        await _repo.Add(CreateLearningSetModel());
        await _repo.Add(CreateLearningSetModel());

        var count = await _repo.GetCount();
        Assert.AreEqual(2, count);
    }

    [Test]
    public async Task Update_modelUpdated() {
        var origin = CreateLearningSetModel();
        await _repo.Add(origin);
        var updated = CreateLearningSetModel();
        updated.Id = origin.Id;
        updated.Name = "newOne";
        updated.Passed = 2;
        updated.Used = 3;
        updated.Words[0].WordId = ObjectId.GenerateNewId();
        await _repo.Update(updated);
        var recreated = await _repo.GetOrDefault(updated.Id);
        AssertModelsAreEqual(updated, recreated);
    }


    private static LearningSet CreateLearningSetModel() {
        var model = new LearningSet {
            Enabled = true,
            Name = "qwe",
            Used = 12,
            Passed = 3,
            Words = new List<WordInLearningSet> {
                new WordInLearningSet {
                    AllowedExamples = Array.Empty<ObjectId>(),
                    AllowedTranslations = new[] { "test", "vest" },
                    WordId = new ObjectId(),
                }
            }
        };
        return model;
    }

    private static void AssertModelsAreEqual(LearningSet expected, LearningSet actual) {
        Assert.AreEqual(expected.Enabled, actual.Enabled);
        Assert.AreEqual(expected.Name, actual.Name);
        Assert.AreEqual(expected.Id, actual.Id);
        Assert.AreEqual(expected.Passed, actual.Passed);
        Assert.AreEqual(expected.Used, actual.Used);

        Assert.AreEqual(expected.Words.Count, actual.Words.Count);
        Assert.AreEqual(expected.Words[0].Id, actual.Words[0].Id);
        Assert.AreEqual(expected.Words[0].AllowedExamples.Length, actual.Words[0].AllowedExamples.Length);
        Assert.AreEqual(expected.Words[0].AllowedTranslations.Length, actual.Words[0].AllowedTranslations.Length);
        Assert.AreEqual(expected.Words[0].AllowedTranslations[1], actual.Words[0].AllowedTranslations[1]);
        Assert.AreEqual(expected.Words[0].WordId, actual.Words[0].WordId);
    }
}

}