using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.WordKits;

public class LearningSetsRepo : IMongoRepo {
    private readonly IMongoDatabase _db;
    private IMongoCollection<LearningSet> Collection
        => _db.GetCollection<LearningSet>("learningSets");
    public LearningSetsRepo(IMongoDatabase db) => _db = db;

    public Task Add(LearningSet model) => Collection.InsertOneAsync(model);

    public Task Update(LearningSet user) => Collection.ReplaceOneAsync(o => o.Id == user.Id, user);

    public Task<LearningSet> GetOrDefault(ObjectId id) =>
        Collection
            .Find(Builders<LearningSet>.Filter.Eq("Id", id))
            .FirstOrDefaultAsync();

    public Task UpdateDb() => Task.CompletedTask;

    public Task<List<LearningSet>> GetAll() => Collection
                                                    .Find(Builders<LearningSet>.Filter.Empty)
                                                    .ToListAsync();

    public Task<long> GetCount() => Collection.CountDocumentsAsync(new BsonDocument());
}