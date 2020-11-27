using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SayWhat.MongoDAL.Users
{
    public class User
    {
        public User() { }

        public User(long telegramId,string firstName, string lastName, string telegramNick)
        {
            TelegramId = telegramId;
            TelegramNick = telegramNick;
            TelegramFirstName = firstName;
            TelegramLastName = lastName;
            Id = ObjectId.GenerateNewId();
        }
        // ReSharper disable once InconsistentNaming
        public ObjectId Id { get; set; }

        [BsonElement(UsersRepo.UserTelegramIdFieldName)]
        public long? TelegramId { get; set; }
        [BsonElement("tfn")]
        public string TelegramFirstName{ get; set; }
        [BsonElement("tln")]
        public string TelegramLastName{ get; set; }
        [BsonElement("tn")]
        public string TelegramNick { get; set; }
        [BsonElement("src")]
        public UserSource Source { get; set; }
        [BsonElement("wc")]
        public int WordsCount { get; set; }
        [BsonElement("pc")]
        public int PairsCount { get; set; }
        [BsonElement("ec")]
        public int ExamplesCount { get; set; }
    }
 
    public enum UserSource
    {
        Unknown = 0,
        Telegram = 1,
    }
}