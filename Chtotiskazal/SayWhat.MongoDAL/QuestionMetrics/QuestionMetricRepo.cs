using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MongoDB.Driver;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Users;

namespace SayWhat.Bll
{
    public class QuestionMetricRepo: IMongoRepo
    {
        private IMongoDatabase _db;
        public QuestionMetricRepo(IMongoDatabase db) => _db = db;

        public Task Add(QuestionMetric metric) => Collection.InsertOneAsync(metric);

        public Task UpdateDb() => Task.CompletedTask;
        private IMongoCollection<QuestionMetric> Collection 
            => _db.GetCollection<QuestionMetric>("questionMetrics");
    }
}