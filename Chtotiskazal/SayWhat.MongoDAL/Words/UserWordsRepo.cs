using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using SayWhat.MongoDAL.Users;

namespace SayWhat.MongoDAL.Words
{
    public class UserWordsRepo : IMongoRepo
    {
        private static readonly Random Rnd = new Random(DateTime.Now.Millisecond);

        public const string UserCollectionName = "words";
        public const string CurrentScoreFieldName = "cur";
        public const string AbsoluteScoreFieldName = "abs";
        public const string UserIdFieldName = "uid";
        public const string PassedScoreFieldName = "scr";
        public const string OriginWordFieldName = "w";
        public const string LastUpdateScoreTime = "updt";

        private readonly IMongoDatabase _db;

        public UserWordsRepo(IMongoDatabase db) => _db = db;

        public Task Add(UserWord word) => Collection.InsertOneAsync(word);

        public Task<List<UserWord>> GetWorstLearned(User user, int maxCount)
            => Collection
                .Find(Builders<UserWord>.Filter.Eq(UserIdFieldName, user._id))
                .SortBy(a=>a.CurrentScore)
                .Limit(maxCount)
                .ToListAsync();
         
        public Task<List<UserWord>> GetWorstLearned(User user, int maxCount, int minimumLearnRate)
            => Collection
                .Find(Builders<UserWord>.Filter.And(
                    Builders<UserWord>.Filter.Eq(UserIdFieldName, user._id),
                    Builders<UserWord>.Filter.Gt(PassedScoreFieldName, minimumLearnRate)
                ))
                .SortBy(a=>a.CurrentScore)
                .Limit(maxCount)
                .ToListAsync();


        public Task<List<UserWord>> GetAllUserWordsAsync(User user)
            => Collection
                .Find(Builders<UserWord>.Filter.Eq(UserIdFieldName, user._id))
                .ToListAsync();

        public Task UpdateMetrics(UserWord word)
        {
            var updateDef = Builders<UserWord>.Update
                .Set(o => o.ScoreUpdatedTimestamp,   DateTime.Now)
                .Set(o => o.AbsoluteScore,   word.AbsoluteScore)
                .Set(o => o.CurrentScore,   word.CurrentScore)
                .Set(o => o.QuestionPassed,   word.QuestionPassed)
                .Set(o => o.QuestionAsked, word.QuestionAsked)
                .Set(o => o.LastQuestionTimestamp,   word.LastQuestionTimestamp);

            return Collection.UpdateOneAsync(o => o._id == word._id, updateDef);
        }

        private IMongoCollection<UserWord> Collection
            => _db.GetCollection<UserWord>(UserCollectionName);
        public async Task UpdateDb()
        {
            await Collection.Indexes.DropAllAsync();
            var csKeys = Builders<UserWord>.IndexKeys.Ascending(CurrentScoreFieldName);
            var csIndexOptions = new CreateIndexOptions<UserWord> {
                Unique = false 
            };
            var currentScoreIndex = new CreateIndexModel<UserWord>(csKeys, csIndexOptions);
            
            var rndKeys = Builders<UserWord>.IndexKeys.Ascending(LastUpdateScoreTime);
            var rndIndexOptions = new CreateIndexOptions<UserWord> {Unique = false};
            var rndScoreIndex = new CreateIndexModel<UserWord>(rndKeys, rndIndexOptions);
            
            await Collection.Indexes.CreateManyAsync(new[]{currentScoreIndex, rndScoreIndex});
        }


        public Task Update(UserWord entity)
        {
            entity.ScoreUpdatedTimestamp = DateTime.Now;
            return Collection.FindOneAndReplaceAsync(f => f._id == entity._id, entity);
        }
        
        public async Task<bool> HasAnyFor(User user)
        {
            var docsCount = await Collection
                .Find(Builders<UserWord>.Filter.Eq(UserIdFieldName, user._id))
                .CountDocumentsAsync();
            return docsCount > 0;
        }

        public Task<UserWord> GetWordOrDefault(User user, string enWord)
        =>
            Collection
                .Find(Builders<UserWord>.Filter.And(
                    Builders<UserWord>.Filter.Eq(UserIdFieldName, user._id),
                    Builders<UserWord>.Filter.Eq(OriginWordFieldName, enWord)
                )).FirstOrDefaultAsync();

        /// <summary>
        /// Returns users words that was updated oldest
        /// </summary>
        public Task<List<UserWord>> GetOldestUpdatedWords(User user, int count) =>
            Collection
                .Find(Builders<UserWord>.Filter.Eq(UserIdFieldName, user._id))
                .SortBy(a=>a.ScoreUpdatedTimestamp)
                .Limit(count)
                .ToListAsync();
    }
}