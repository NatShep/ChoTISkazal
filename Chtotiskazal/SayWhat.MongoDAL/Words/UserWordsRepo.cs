using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using SayWhat.MongoDAL.Users;

namespace SayWhat.MongoDAL.Words
{
    public class UserWordsRepo : IMongoRepo
    {
        public const string UserCollectionName = "words";
        public const string CurrentRatingFieldName = "currentRate";
        public const string UserIdFieldName = "userId";

        private readonly IMongoDatabase _db;

        public UserWordsRepo(IMongoDatabase db) => _db = db;

        public Task Add(UserWord word) => Collection.InsertOneAsync(word);

        public Task<List<UserWord>> GetWorstLearned(User user, int maxCount)
            => Collection
                .Find(Builders<UserWord>.Filter.Eq(UserIdFieldName, user.Id))
                .SortBy(a=>a.CurrentRate)
                .Limit(maxCount)
                .ToListAsync();

        public Task<List<UserWord>> GetAllUserWordsAsync(User user)
            => Collection
                .Find(Builders<UserWord>.Filter.Eq(UserIdFieldName, user.Id))
                .ToListAsync();

        public Task UpdateScores(UserWord word)
        {
            var updateDef = Builders<UserWord>.Update
                .Set(o => o.CurrentRate, word.CurrentRate)
                .Set(o => o.AbsoluteRate, word.AbsoluteRate)
                .Set(o => o.ExamsPassed, word.CurrentRate);

            return Collection.UpdateOneAsync(o => o.Id == word.Id, updateDef);
        }

        private IMongoCollection<UserWord> Collection
            => _db.GetCollection<UserWord>(UserCollectionName);
        public async Task UpdateDb()
        {
            await Collection.Indexes.DropAllAsync();
            var keys = Builders<UserWord>.IndexKeys.Ascending(CurrentRatingFieldName);
            var indexOptions = new CreateIndexOptions<UserWord> {
                Unique = false 
            };
            var model = new CreateIndexModel<UserWord>(keys, indexOptions);
            await Collection.Indexes.CreateOneAsync(model);
        }
    }
}