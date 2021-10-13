using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.WordKits;

public class LearningSetsRepo : IMongoRepo {
    private readonly IMongoDatabase _db;
    private IMongoCollection<LearningSetModel> Collection
        => _db.GetCollection<LearningSetModel>("setsOfWords");
    public LearningSetsRepo(IMongoDatabase db) => _db = db;

    public Task Add(LearningSetModel model) => Collection.InsertOneAsync(model);

    public Task Update(LearningSetModel user) => Collection.ReplaceOneAsync(o => o.Id == user.Id, user);

    public Task<LearningSetModel> GetOrDefault(ObjectId id) =>
        Collection
            .Find(Builders<LearningSetModel>.Filter.Eq("Id", id))
            .FirstOrDefaultAsync();

    public Task UpdateDb() => Task.CompletedTask;

    public Task<List<LearningSetModel>> GetAll() => Collection
                                                    .Find(Builders<LearningSetModel>.Filter.Empty)
                                                    .ToListAsync();

    public Task<long> GetCount() => Collection.CountDocumentsAsync(new BsonDocument());
}