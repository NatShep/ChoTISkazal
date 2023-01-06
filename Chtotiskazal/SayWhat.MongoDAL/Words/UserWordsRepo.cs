using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using SayWhat.MongoDAL.Users;

namespace SayWhat.MongoDAL.Words {

public class UserWordsRepo : IMongoRepo {
    public const string UserCollectionName = "words";
    public const string CurrentScoreFieldName = "cur";
    public const string AbsoluteScoreFieldName = "abs";
    public const string QuestionAskedFieldName = "qa";
    public const string QuestionPassedFieldName = "qp";
    public const string UserIdFieldName = "uid";
    public const string OriginWordFieldName = "w";
    public const string LastUpdateScoreTime = "updt";
    public const string LastQuestionAskedTimestampFieldName = "askt";
    private readonly IMongoDatabase _db;

    public UserWordsRepo(IMongoDatabase db) => _db = db;

    public Task Add(UserWordModel word) => Collection.InsertOneAsync(word);

    public Task<List<UserWordModel>> GetWorstLearned(UserModel user, int count)
        => Collection
           .Find(Builders<UserWordModel>.Filter.Eq(UserIdFieldName, user.Id))
           .Sort($"{{{CurrentScoreFieldName}:1}}")
           .Limit(count)
           .ToListAsync();
    
    public Task<List<UserWordModel>> GetWellLearnedOld(UserModel user, int count)
        => Collection
            .Find(Builders<UserWordModel>.Filter.Eq(UserIdFieldName, user.Id))
            .Sort($"{{{CurrentScoreFieldName}:1}}")
            .Limit(count)
            .ToListAsync();

    public Task<List<UserWordModel>> GetWorstLearned(UserModel user, int count, int minimumLearnRate)
        => Collection
           .Find(
               Builders<UserWordModel>.Filter.And(
                   Builders<UserWordModel>.Filter.Eq(UserIdFieldName, user.Id),
                   Builders<UserWordModel>.Filter.Gt(AbsoluteScoreFieldName, minimumLearnRate)
               ))
           .Sort($"{{{UserWordsRepo.CurrentScoreFieldName}: 1}}")
           .Limit(count)
           .ToListAsync();
    
    public Task<List<UserWordModel>> GetWellLearned(UserModel user, int maxCount)
        => Collection
            .Find(
                Builders<UserWordModel>.Filter.And(
                    Builders<UserWordModel>.Filter.Eq(UserIdFieldName, user.Id),
                    Builders<UserWordModel>.Filter.Gt(AbsoluteScoreFieldName, WordLeaningGlobalSettings.LearnedWordMinScore),
                    Builders<UserWordModel>.Filter.Lt(AbsoluteScoreFieldName, WordLeaningGlobalSettings.WellDoneWordMinScore)
                ))
            .Sort($"{{{UserWordsRepo.CurrentScoreFieldName}: 1}}")
            .Limit(maxCount)
            .ToListAsync();
    
    public Task<List<UserWordModel>> GetNotWellLearned(UserModel user, int maxCount)
        => Collection
            .Find(
                Builders<UserWordModel>.Filter.And(
                    Builders<UserWordModel>.Filter.Eq(UserIdFieldName, user.Id),
                    Builders<UserWordModel>.Filter.Lt(AbsoluteScoreFieldName, WordLeaningGlobalSettings.LearnedWordMinScore)
                ))
            .Sort($"{{{UserWordsRepo.CurrentScoreFieldName}: -1}}")
            .Limit(maxCount)
            .ToListAsync();

    public Task<List<UserWordModel>> GetAllBestLearned(UserModel user)
        => Collection
            .Find(
                Builders<UserWordModel>.Filter.And(
                    Builders<UserWordModel>.Filter.Eq(UserIdFieldName, user.Id),
                    Builders<UserWordModel>.Filter.Gte(AbsoluteScoreFieldName,
                        WordLeaningGlobalSettings.WellDoneWordMinScore)
                ))
            .ToListAsync();
    
    public Task<List<UserWordModel>> GetLastAsked(UserModel user, int maxCount, int minimumQuestionAsked)
        => Collection
           .Find(
               Builders<UserWordModel>.Filter.And(
                   Builders<UserWordModel>.Filter.Eq(UserIdFieldName, user.Id),
                   Builders<UserWordModel>.Filter.Gt(QuestionAskedFieldName, minimumQuestionAsked),
                   Builders<UserWordModel>.Filter.Lt(AbsoluteScoreFieldName, WordLeaningGlobalSettings.WellDoneWordMinScore)
    ))
           .Sort($"{{{UserWordsRepo.LastUpdateScoreTime}:-1}}")
           .Limit(maxCount)
           .ToListAsync();

    public Task<List<UserWordModel>> GetAllUserWordsAsync(UserModel user)
        => Collection
           .Find(Builders<UserWordModel>.Filter.Eq(UserIdFieldName, user.Id))
           .ToListAsync();

    public Task UpdateMetrics(UserWordModel word) {
        var updateDef = Builders<UserWordModel>.Update
                                               .Set(LastUpdateScoreTime, DateTime.Now)
                                               .Set(AbsoluteScoreFieldName, word.AbsoluteScore)
                                               .Set(CurrentScoreFieldName, word.CurrentOrderScore)
                                               .Set(QuestionPassedFieldName, word.QuestionPassed)
                                               .Set(QuestionAskedFieldName, word.QuestionAsked)
                                               .Set(
                                                   LastQuestionAskedTimestampFieldName,
                                                   word.LastQuestionAskedTimestamp);

        return Collection.UpdateOneAsync(o => o.Id == word.Id, updateDef);
    }

    private IMongoCollection<UserWordModel> Collection
        => _db.GetCollection<UserWordModel>(UserCollectionName);

    public async Task UpdateDb() {
        await Collection.Indexes.DropAllAsync();
        var csKeys = Builders<UserWordModel>.IndexKeys.Ascending(CurrentScoreFieldName);
        var csIndexOptions = new CreateIndexOptions<UserWordModel> {
            Unique = false
        };
        var currentScoreIndex = new CreateIndexModel<UserWordModel>(csKeys, csIndexOptions);

        var rndKeys = Builders<UserWordModel>.IndexKeys.Ascending(LastUpdateScoreTime);
        var rndIndexOptions = new CreateIndexOptions<UserWordModel> { Unique = false };
        var rndScoreIndex = new CreateIndexModel<UserWordModel>(rndKeys, rndIndexOptions);

        await Collection.Indexes.CreateManyAsync(new[] { currentScoreIndex, rndScoreIndex });
    }


    public Task Update(UserWordModel entity) {
        entity.UpdateCurrentScore();
        return Collection.FindOneAndReplaceAsync(f => f.Id == entity.Id, entity);
    }

    public async Task<bool> HasAnyFor(UserModel user) {
        var docsCount = await Collection
                              .Find(Builders<UserWordModel>.Filter.Eq(UserIdFieldName, user.Id))
                              .CountDocumentsAsync();
        return docsCount > 0;
    }

    public Task<UserWordModel> GetWordOrDefault(UserModel user, string enWord)
        =>
            Collection
                .Find(
                    Builders<UserWordModel>.Filter.And(
                        Builders<UserWordModel>.Filter.Eq(UserIdFieldName, user.Id),
                        Builders<UserWordModel>.Filter.Eq(OriginWordFieldName, enWord)
                    ))
                .FirstOrDefaultAsync();

    public Task<bool> Contains(UserModel user, string enWord) =>
        Collection
            .Find(
                Builders<UserWordModel>.Filter.And(
                    Builders<UserWordModel>.Filter.Eq(UserIdFieldName, user.Id),
                    Builders<UserWordModel>.Filter.Eq(OriginWordFieldName, enWord)
                ))
            .AnyAsync();

    /// <summary>
    /// Returns users words that was updated oldest
    /// </summary>
    public Task<List<UserWordModel>> GetOldestUpdatedWords(UserModel user, int count) =>
        Collection
            .Find(Builders<UserWordModel>.Filter.Eq(UserIdFieldName, user.Id))
            .Sort($"{{{UserWordsRepo.LastUpdateScoreTime}:1}}")
            .Limit(count)
            .ToListAsync();

    public async Task<IReadOnlyCollection<UserWordModel>> GetAllWords(UserModel user) {
        var c = await Collection.FindAsync(Builders<UserWordModel>.Filter.Eq(UserIdFieldName, user.Id));
        return await c.ToListAsync();
    }

    public Task Remove(UserWordModel model)
        => Collection.DeleteOneAsync(Builders<UserWordModel>.Filter.Eq("Id", model.Id));
}

}