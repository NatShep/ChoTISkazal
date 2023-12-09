using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace SayWhat.MongoDAL.QuestionMetrics;

public class QuestionMetricRepo : IMongoRepo {
    private readonly IMongoDatabase _db;
    public QuestionMetricRepo(IMongoDatabase db) => _db = db;

    public Task Add(QuestionMetric metric) => Collection.InsertOneAsync(metric);

    public Task<List<QuestionMetric>> GetFrom(DateTime from)
        => Collection
            .Find(Builders<QuestionMetric>.Filter.Gt("_id", ObjectId.GenerateNewId(from)))
            .ToListAsync();

    public Task UpdateDb() => Task.CompletedTask;

    private IMongoCollection<QuestionMetric> Collection
        => _db.GetCollection<QuestionMetric>("questionMetrics");
}