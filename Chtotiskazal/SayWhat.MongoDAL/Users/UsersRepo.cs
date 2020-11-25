#nullable enable
using System;
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
        
        public Task Add(User user) => Collection.InsertOneAsync(user);
        
        public Task<User> GetOrDefaultByTelegramId(long telegramId) => 
            Collection
                .Find(Builders<User>.Filter.Eq(UserTelegramIdFieldName, telegramId))
                .FirstOrDefaultAsync();

        public async Task<User> AddFromTelegram(long telegramId, string? nick)
        {
            var newUser = new User
            {
                TelegramId = telegramId,
                TelegramNick = nick,
                Source = UserSource.Telegram
            };
            
            await Collection.InsertOneAsync(newUser);
             var user = await GetOrDefaultByTelegramId(telegramId);
             return  user??throw new InvalidOperationException("Add user was failed.");
        }

        private IMongoCollection<User> Collection 
            => _db.GetCollection<User>(UserCollectionName);
        
        public async Task UpdateDb()
        {
            await Collection.Indexes.DropAllAsync();
            var keys = Builders<User>.IndexKeys.Ascending(UserTelegramIdFieldName);
            var indexOptions = new CreateIndexOptions<User>
            {
                Unique = true ,
                PartialFilterExpression = Builders<User>.Filter.Type(u=>u.TelegramId, BsonType.Int64)
            };
            var model = new CreateIndexModel<User>(keys, indexOptions);
            await Collection.Indexes.CreateOneAsync(model);
        }
        public Task<long> GetCount() => Collection.CountDocumentsAsync(new BsonDocument());
    }
}