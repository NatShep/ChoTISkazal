using System.Threading.Tasks;
using MongoDB.Driver;

namespace SayWhat.MongoDAL.QuestionMetrics {

public class QuestionMetricRepo : IMongoRepo {
    private readonly IMongoDatabase _db;
    public QuestionMetricRepo(IMongoDatabase db) => _db = db;

    public Task Add(QuestionMetric metric) => Collection.InsertOneAsync(metric);

    public Task UpdateDb() => Task.CompletedTask;
    private IMongoCollection<QuestionMetric> Collection
        => _db.GetCollection<QuestionMetric>("questionMetrics");
}

}