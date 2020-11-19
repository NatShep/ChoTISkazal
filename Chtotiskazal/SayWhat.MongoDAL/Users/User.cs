using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SayWhat.MongoDAL.Users
{
    public class User
    {
        public User()
        {
            
        }

        public User(long telegramId, string nick)
        {
            this.TelegramId = telegramId;
            this.Nick = nick;
            Id = ObjectId.GenerateNewId();
        }
        public ObjectId Id { get; set; }

        [BsonElement(UsersRepo.UserTelegramIdFieldName)]
        public long? TelegramId { get; set; }
        
        public string? Nick { get; set; }
        public UserSource Source { get; set; }
        
    }

    public enum UserSource
    {
        Unknown = 0,
        Telegram = 1,
    }
}