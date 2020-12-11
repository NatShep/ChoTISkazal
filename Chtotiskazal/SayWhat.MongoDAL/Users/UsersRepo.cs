#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace SayWhat.MongoDAL.Users
{
    public class UsersRepo: IMongoRepo
    {
        public const string UserCollectionName = "users";
        public const string UserTelegramIdFieldName = "TelegramId";
        private readonly IMongoDatabase _db;

        public UsersRepo(IMongoDatabase db) => _db = db;
        
        public Task Add(UserModel user) => Collection.InsertOneAsync(user);
        
        public Task<UserModel> GetOrDefaultByTelegramIdOrNull(long telegramId) => 
            Collection
                .Find(Builders<UserModel>.Filter.Eq(UserTelegramIdFieldName, telegramId))
                .FirstOrDefaultAsync();

        public async Task<UserModel> AddFromTelegram(long telegramId,string firstName, string lastName, string? nick)
        {
            var newUser = new UserModel(
                telegramId: telegramId, 
                firstName: firstName, 
                lastName: lastName, 
                telegramNick: nick, 
                source: UserSource.Telegram);
            
            await Collection.InsertOneAsync(newUser);
             var user = await GetOrDefaultByTelegramIdOrNull(telegramId);
             return  user??throw new InvalidOperationException("Add user was failed.");
        }

        private IMongoCollection<UserModel> Collection 
            => _db.GetCollection<UserModel>(UserCollectionName);
        
        public async Task UpdateDb()
        {
            await Collection.Indexes.DropAllAsync();
            var keys = Builders<UserModel>.IndexKeys.Ascending(UserTelegramIdFieldName);
            var indexOptions = new CreateIndexOptions<UserModel>
            {
                Unique = true ,
                PartialFilterExpression = Builders<UserModel>.Filter.Type(UserTelegramIdFieldName, BsonType.Int64)
            };
            var model = new CreateIndexModel<UserModel>(keys, indexOptions);
            await Collection.Indexes.CreateOneAsync(model);
        }
        public Task<long> GetCount() => Collection.CountDocumentsAsync(new BsonDocument());
        public Task Update(UserModel user) => Collection.ReplaceOneAsync(o => o.Id == user.Id, user);

        public Task UpdateCounters(UserModel user)
        {
            var updateDef = Builders<UserModel>.Update
                .Set(o => o.WordsCount, user.WordsCount)
                .Set(o => o.PairsCount, user.PairsCount)
                .Set(o => o.ExamplesCount, user.ExamplesCount)
                .Set(o => o.EnglishWordTranslationRequestsCount, user.EnglishWordTranslationRequestsCount)
                .Set(o => o.RussianWordTranslationRequestsCount, user.RussianWordTranslationRequestsCount);


            return Collection.UpdateOneAsync(o => o.Id == user.Id, updateDef);
        }

        public List<UserModel> GetAll()
        {
            return Collection.AsQueryable().ToList();
        }
    }
}