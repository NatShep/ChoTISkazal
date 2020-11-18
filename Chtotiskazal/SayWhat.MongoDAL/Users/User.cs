using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SayWa.MongoDAL.Users
{
    public class User
    {
        public ObjectId Id { get; set; }

        [BsonElement(UsersRepository.UserTelegramIdFieldName)]
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