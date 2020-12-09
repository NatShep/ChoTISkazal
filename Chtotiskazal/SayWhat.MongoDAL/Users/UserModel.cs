using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable ConvertToAutoProperty
#pragma warning disable 649

namespace SayWhat.MongoDAL.Users
{
    [BsonIgnoreExtraElements]
    public class UserModel
    {
        public UserModel() { }

        public UserModel(long? telegramId,string firstName, string lastName, string telegramNick, UserSource source)
        {
            _source = source;
            _telegramId = telegramId;
            _telegramNick = telegramNick;
            _telegramFirstName = firstName;
            _telegramLastName = lastName;
            RegistrateActivity();
            Id = ObjectId.GenerateNewId();
        }
        public ObjectId Id { get; set; }
        [BsonElement("a")]
        private DateTime _lastActivity;
        [BsonIgnore]
        public DateTime LastActivity => _lastActivity;

        [BsonElement(UsersRepo.UserTelegramIdFieldName)]
        private long? _telegramId = null;
        [BsonIgnore]
        public long? TelegramId => _telegramId;

        [BsonElement("tfn")] private string _telegramFirstName;

        [BsonIgnore]
        public string TelegramFirstName => _telegramFirstName;
        
        
        [BsonElement("tln")] private string _telegramLastName;
        [BsonIgnore]
        public string TelegramLastName => _telegramLastName;
        
        
        [BsonElement("tn")] private string _telegramNick;
        [BsonIgnore]
        public string TelegramNick => _telegramNick;
        
        
        [BsonElement("src")] private UserSource _source;
        [BsonIgnore]
        public UserSource Source => _source;

        
        [BsonElement("wc")] private int _wordsCount;
        [BsonIgnore]
        public int WordsCount => _wordsCount;

        
        [BsonElement("pc")] private int _pairsCount;
        [BsonIgnore]
        public int PairsCount => _pairsCount;

        
        [BsonElement("ec")] private int _examplesCount;
        [BsonIgnore]
        public int ExamplesCount => _examplesCount;
        
        
        [BsonElement("en_wtc")] private int _englishWordTranslationRequestsCount;
        [BsonIgnore]
        public int EnglishWordTranslationRequestsCount => _englishWordTranslationRequestsCount;

        
        
        [BsonElement("ru_wtc")] private int _russianWordTranslationRequestsCount;
        [BsonIgnore]
        public int RussianWordTranslationRequestsCount => _russianWordTranslationRequestsCount;

        public void RegistrateActivity()
        {
            _lastActivity = DateTime.Now;
        }
        public void OnNewWordAdded(int pairsCount, int examplesCount)
        {
            _wordsCount++;
            OnPairsAdded(pairsCount, examplesCount);
        }
        public void OnPairsAdded(int pairsCount, int examplesCount)
        {
            _pairsCount    += pairsCount;
            _examplesCount += examplesCount;
            RegistrateActivity();
        }

        public void IncrementEnglishWordTranlationRequestsCount()
        {
            _englishWordTranslationRequestsCount++;
            RegistrateActivity();
        }
        public void IncrementRussianWordTranlationRequestsCount()
        {
            _russianWordTranslationRequestsCount++;
            RegistrateActivity();
        }
        public void OnQuestionPassed()
        {
            RegistrateActivity();
        }
        public void OnQuestionFailed()
        {
            RegistrateActivity();
        }
        public void OnExamPassed()
        {
            RegistrateActivity();
        }
        public void UpdateScore(int newScore)
        {
            RegistrateActivity();
        }
        
        [BsonDefaultValue(null)]
        [BsonIgnoreIfDefault]
        [BsonElement("lds")]
        public DailyStats[] LastDaysStats { get; set; }
        
        [BsonDefaultValue(null)]
        [BsonIgnoreIfDefault]
        [BsonElement("lms")]
        public MonthsStats[] LastMonthStats { get; set; }
        
        [BsonElement("stats")]
        public TotalStats TotalStats { get; set; }
    }
 
    public enum UserSource
    {
        Unknown = 0,
        Telegram = 1,
    }
}