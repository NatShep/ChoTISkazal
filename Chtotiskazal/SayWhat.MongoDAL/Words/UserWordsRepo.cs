using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using SayWhat.MongoDAL.Users;

namespace SayWhat.MongoDAL.Words
{
    public class UserWordsRepo : IMongoRepo
    {
        public const string UserCollectionName = "words";
        public const string CurrentRatingFieldName = "currentRate";
        public const string UserIdFieldName = "userId";
        public const string PassedScoreFieldName = "passedScore";
        public const string OriginWordFieldName = "word";

        private readonly IMongoDatabase _db;

        public UserWordsRepo(IMongoDatabase db) => _db = db;

        public Task Add(UserWord word) => Collection.InsertOneAsync(word);

        public Task<List<UserWord>> GetWorstLearned(User user, int maxCount)
            => Collection
                .Find(Builders<UserWord>.Filter.Eq(UserIdFieldName, user.Id))
                .SortBy(a=>a.CurrentRate)
                .Limit(maxCount)
                .ToListAsync();
         
        public Task<List<UserWord>> GetWorstLearned(User user, int maxCount, int minimumLearnRate)
            => Collection
                .Find(Builders<UserWord>.Filter.And(
                    Builders<UserWord>.Filter.Eq(UserIdFieldName, user.Id),
                    Builders<UserWord>.Filter.Gt(PassedScoreFieldName, minimumLearnRate)
                ))
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
                .Set(o => o.PassedScore,   word.PassedScore)
                .Set(o => o.AggregateScore, word.AggregateScore)
                .Set(o => o.LastExam,   word.LastExam);

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

        public async Task<IEnumerable<UserWord>> GetRandowWords(User user, int count)
        {
            return new UserWord[0];
            /*
             *db.mycoll.aggregate([
                { $match: { a: 10 } },
                { $sample: { size: 1 } }
            ])
             * 
             */
            //Collection.Find(Builders<UserWord>.Filter.Eq(UserIdFieldName, user.Id))
              //  .
 
            
            //var course = courses.FindAs<Course>(query1).SetFields("Title","Description").ToList();
            
            //Collection.Aggregate()
            //throw new System.NotImplementedException();
        }

        public Task Update(UserWord entity) =>
            Collection.FindOneAndReplaceAsync(f=>f.Id==entity.Id, entity);

        public async Task<bool> HasAnyFor(User user)
        {
            var docsCount = await Collection
                .Find(Builders<UserWord>.Filter.Eq(UserIdFieldName, user.Id))
                .CountDocumentsAsync();
            return docsCount > 0;
        }

        public Task<UserWord> GetWordOrDefault(User user, string enWord)
        =>
            Collection
                .Find(Builders<UserWord>.Filter.And(
                    Builders<UserWord>.Filter.Eq(UserIdFieldName, user.Id),
                    Builders<UserWord>.Filter.Eq(OriginWordFieldName, enWord)
                )).FirstOrDefaultAsync();
        
    }
}