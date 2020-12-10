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

        public Task<List<UserWordModel>> GetWorstLearned(UserModel user, int maxCount)
            => Collection
                .Find(Builders<UserWordModel>.Filter.Eq(UserIdFieldName, user.Id))
                .Sort($"{{{UserWordsRepo.CurrentScoreFieldName}:1}}" )
                .Limit(maxCount)
                .ToListAsync();
         
        public Task<List<UserWordModel>> GetWorstLearned(UserModel user, int maxCount, int minimumLearnRate)
            => Collection
                .Find(Builders<UserWordModel>.Filter.And(
                    Builders<UserWordModel>.Filter.Eq(UserIdFieldName, user.Id),
                    Builders<UserWordModel>.Filter.Gt(AbsoluteScoreFieldName, minimumLearnRate)
                ))
                .Sort($"{{{UserWordsRepo.CurrentScoreFieldName}: 1}}")
                .Limit(maxCount)
                .ToListAsync();

        public Task<List<UserWordModel>> Get(UserModel user, int maxCount, int minimumQuestionAsked)
            => Collection
                .Find(Builders<UserWordModel>.Filter.And(
                    Builders<UserWordModel>.Filter.Eq(UserIdFieldName, user.Id),
                    Builders<UserWordModel>.Filter.Gt(QuestionAskedFieldName, minimumQuestionAsked)
                ))
                .Sort($"{{{UserWordsRepo.LastUpdateScoreTime}:1}}")
                .Limit(maxCount)
                .ToListAsync();

        public Task<List<UserWordModel>> GetAllUserWordsAsync(UserModel user)
            => Collection
                .Find(Builders<UserWordModel>.Filter.Eq(UserIdFieldName, user.Id))
                .ToListAsync();

        public Task UpdateMetrics(UserWordModel word)
        {
            var updateDef = Builders<UserWordModel>.Update
                .Set(LastUpdateScoreTime,   DateTime.Now)
                .Set(AbsoluteScoreFieldName,   word.AbsoluteScore)
                .Set(CurrentScoreFieldName,   word.CurrentOrderScore)
                .Set(QuestionPassedFieldName,   word.QuestionPassed)
                .Set(QuestionAskedFieldName, word.QuestionAsked)
                .Set(LastQuestionAskedTimestampFieldName,   word.LastQuestionTimestamp);

            return Collection.UpdateOneAsync(o => o.Id == word.Id, updateDef);
        }

        private IMongoCollection<UserWordModel> Collection
            => _db.GetCollection<UserWordModel>(UserCollectionName);
        public async Task UpdateDb()
        {
            await Collection.Indexes.DropAllAsync();
            var csKeys = Builders<UserWordModel>.IndexKeys.Ascending(CurrentScoreFieldName);
            var csIndexOptions = new CreateIndexOptions<UserWordModel> {
                Unique = false 
            };
            var currentScoreIndex = new CreateIndexModel<UserWordModel>(csKeys, csIndexOptions);
            
            var rndKeys = Builders<UserWordModel>.IndexKeys.Ascending(LastUpdateScoreTime);
            var rndIndexOptions = new CreateIndexOptions<UserWordModel> {Unique = false};
            var rndScoreIndex = new CreateIndexModel<UserWordModel>(rndKeys, rndIndexOptions);
            
            await Collection.Indexes.CreateManyAsync(new[]{currentScoreIndex, rndScoreIndex});
        }


        public Task Update(UserWordModel entity)
        {
            entity.UpdateCurrentScore();
            return Collection.FindOneAndReplaceAsync(f => f.Id == entity.Id, entity);
        }
        
        public async Task<bool> HasAnyFor(UserModel user)
        {
            var docsCount = await Collection
                .Find(Builders<UserWordModel>.Filter.Eq(UserIdFieldName, user.Id))
                .CountDocumentsAsync();
            return docsCount > 0;
        }

        public Task<UserWordModel> GetWordOrDefault(UserModel user, string enWord)
        =>
            Collection
                .Find(Builders<UserWordModel>.Filter.And(
                    Builders<UserWordModel>.Filter.Eq(UserIdFieldName, user.Id),
                    Builders<UserWordModel>.Filter.Eq(OriginWordFieldName, enWord)
                )).FirstOrDefaultAsync();

        /// <summary>
        /// Returns users words that was updated oldest
        /// </summary>
        public Task<List<UserWordModel>> GetOldestUpdatedWords(UserModel user, int count) =>
            Collection
                .Find(Builders<UserWordModel>.Filter.Eq(UserIdFieldName, user.Id))
                .Sort($"{{{UserWordsRepo.LastUpdateScoreTime}:1}}")
                .Limit(count)
                .ToListAsync();

        
        //DELETE This after
        public IReadOnlyCollection<UserWordModel> GetTestWords(UserModel user)
        {
            var learningWords = new List<UserWordModel>();
            learningWords.AddRange(Collection.Find(Builders<UserWordModel>.Filter.And(
                Builders<UserWordModel>.Filter.Eq(UserIdFieldName,user.Id),
                Builders<UserWordModel>.Filter.Eq(OriginWordFieldName,"bother"))).ToList());
            learningWords.AddRange(Collection.Find(Builders<UserWordModel>.Filter.And(
                Builders<UserWordModel>.Filter.Eq(UserIdFieldName,user.Id),
                Builders<UserWordModel>.Filter.Eq(OriginWordFieldName,"disturb"))).ToList());
            learningWords.AddRange(Collection.Find(Builders<UserWordModel>.Filter.And(
                Builders<UserWordModel>.Filter.Eq(UserIdFieldName,user.Id),
                Builders<UserWordModel>.Filter.Eq(OriginWordFieldName,"enable"))).ToList());
            learningWords.AddRange(Collection.Find(Builders<UserWordModel>.Filter.And(
                Builders<UserWordModel>.Filter.Eq(UserIdFieldName,user.Id),
                Builders<UserWordModel>.Filter.Eq(OriginWordFieldName,"statment"))).ToList());
            return learningWords;
        }
    }
}