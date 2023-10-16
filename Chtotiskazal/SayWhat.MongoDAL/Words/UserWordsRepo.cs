using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;
using MongoDB.Bson;
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
    
    public Task<List<UserWordModel>> GetWordsForLearningBetweenLowAndHighScores(UserModel user,
        int count,
        double lowRate,
        double highRate,
        Func<FieldDefinition<UserWordModel>, SortDefinition<UserWordModel>> sortType)
        => Collection
            .Find(
                Builders<UserWordModel>.Filter.And(
                    Builders<UserWordModel>.Filter.Eq(UserIdFieldName, user.Id),
                    Builders<UserWordModel>.Filter.Gte(AbsoluteScoreFieldName, lowRate),
                    Builders<UserWordModel>.Filter.Lt(AbsoluteScoreFieldName, highRate)
                ))
            .Sort(sortType($"{CurrentScoreFieldName}")) 
            .Limit(count)
            .ToListAsync();
    
    public Task<List<UserWordModel>> GetWordsForLearningAboveScore(UserModel user, int count, double lowRate)
        => Collection
            .Find(
                Builders<UserWordModel>.Filter.And(
                    Builders<UserWordModel>.Filter.Eq(UserIdFieldName, user.Id),
                    Builders<UserWordModel>.Filter.Gte(AbsoluteScoreFieldName, lowRate)
                ))
            .Sort(Builders<UserWordModel>.Sort.Ascending($"{CurrentScoreFieldName}"))
            .Limit(count)
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
                                               .Set(LastQuestionAskedTimestampFieldName,
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


    public async Task Update(UserWordModel entity) {
        entity.RefreshScoreUpdate();
        await Collection.FindOneAndReplaceAsync(f => f.Id == entity.Id, entity);
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

    public async Task<IReadOnlyCollection<UserWordModel>> GetAllWords(UserModel user) {
        var c = await Collection.FindAsync(Builders<UserWordModel>.Filter.Eq(UserIdFieldName, user.Id));
        return await c.ToListAsync();
    }

    public Task Remove(UserWordModel model)
        => Collection.DeleteOneAsync(Builders<UserWordModel>.Filter.Eq("Id", model.Id));
}

}