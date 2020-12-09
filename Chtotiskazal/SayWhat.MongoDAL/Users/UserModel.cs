using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            OnAnyActivity();
            Id = ObjectId.GenerateNewId();
        }
        #region mongo fields
        public ObjectId Id { get; set; }
        
        [BsonElement(UsersRepo.UserTelegramIdFieldName)]
        private long? _telegramId = null;
        [BsonElement("a")]      private DateTime _lastActivity;
        [BsonElement("tfn")]    private string _telegramFirstName;
        [BsonElement("tln")]    private string _telegramLastName;
        [BsonElement("tn")]     private string _telegramNick;
        [BsonElement("src")]    private UserSource _source;
        [BsonElement("wc")]     private int _wordsCount;
        [BsonElement("ec")]     private int _examplesCount;
        [BsonElement("pc")]     private int _pairsCount;
        [BsonElement("en_wtc")] private int _englishWordTranslationRequestsCount;
        [BsonElement("ru_wtc")] private int _russianWordTranslationRequestsCount;
        
        [BsonDefaultValue(null)]
        [BsonIgnoreIfDefault]
        [BsonElement("lds")]
        public Queue<DailyStats> LastDaysStats { get; set; }
        
        [BsonDefaultValue(null)]
        [BsonIgnoreIfDefault]
        [BsonElement("lms")]
        public List<MonthsStats> LastMonthStats { get; set; }
        
        [BsonElement("stats")]
        public TotalStats TotalStats { get; set; }

        [BsonElement("scr")] private int _totalScore; 
        [BsonElement("ldc")] private int _learningDone; 
        [BsonElement("qpc")] private int _questionPassed; 
        [BsonElement("qfc")] private int _questionFailed; 
        #endregion
         public DateTime LastActivity => _lastActivity;
         public long? TelegramId => _telegramId;
         public string TelegramFirstName => _telegramFirstName;
         public string TelegramLastName => _telegramLastName;
         public string TelegramNick => _telegramNick;
         public UserSource Source => _source;
         public int WordsCount => _wordsCount;
         public int PairsCount => _pairsCount;
         public int ExamplesCount => _examplesCount;
         public int EnglishWordTranslationRequestsCount => _englishWordTranslationRequestsCount;
         public int RussianWordTranslationRequestsCount => _russianWordTranslationRequestsCount;

         
        public void OnAnyActivity()
        {
            _lastActivity = DateTime.Now;
        }
        public void OnNewWordAdded(int pairsCount, int examplesCount)
        {
            _wordsCount++;
            var(today, month) = FixStatsAndGetCurrent();

            today.WordsAdded++;
            month.WordsAdded++;
            OnPairsAdded(pairsCount, examplesCount);
        }

        private (DailyStats, MonthsStats) FixStatsAndGetCurrent()
        {
            var today = DateTime.Today;
            DailyStats daily;
            MonthsStats monthly;
            if (LastDaysStats == null || LastDaysStats.Count == 0)
            {
                daily = new DailyStats {Date = today};
                LastDaysStats = new Queue<DailyStats>();
                LastDaysStats.Enqueue(daily);
            }
            else
            {
                var oldest = LastDaysStats.Peek();
                var oldestDelta = (today - oldest.Date);
                if(oldestDelta.TotalDays>30)
                    LastDaysStats.Dequeue();
                var lastDay = LastDaysStats.Last();
                if (lastDay.Date != today)
                {
                    daily = new DailyStats {Date = today};
                    LastDaysStats.Enqueue(daily);
                }
                else
                    daily = lastDay;
            }
            
            var thisMonth = new DateTime(today.Year, today.Month, 1);
            if (LastMonthStats == null || LastMonthStats.Count == 0)
            {
                monthly = new MonthsStats {Date = thisMonth};
                LastMonthStats = new List<MonthsStats>();
                LastMonthStats.Add(monthly);
            }
            else
            {
                monthly = LastMonthStats.Last();
                if (monthly.Date != thisMonth)
                {
                    monthly = new MonthsStats {Date = thisMonth};
                    LastMonthStats.Add(monthly);
                }
            }

            return (daily, monthly);
        }

        public void OnPairsAdded(int pairsCount, int examplesCount)
        {
            _pairsCount    += pairsCount;
            _examplesCount += examplesCount;

            var (today, month) = FixStatsAndGetCurrent();

            today.PairsAdded+= pairsCount;
            today.ExamplesAdded+= examplesCount;

            month.PairsAdded+= pairsCount;
            month.ExamplesAdded+= examplesCount;

            OnAnyActivity();
        }

        public void OnEnglishWordTranslationRequest()
        {
            _englishWordTranslationRequestsCount++;
            OnAnyActivity();
        }
        public void OnRussianWordTranlationRequest()
        {
            _russianWordTranslationRequestsCount++;
            OnAnyActivity();
        }
        public void OnQuestionPassed()
        {
            _questionPassed++;
            var(today, month) = FixStatsAndGetCurrent();

            today.QuestionsPassed++;
            month.QuestionsPassed++;
            OnAnyActivity();
        }
        public void OnQuestionFailed()
        {
            _questionFailed++;
            var(today, month) = FixStatsAndGetCurrent();

            today.QuestionsFailed++;
            month.QuestionsFailed++;
            OnAnyActivity();
        }
        public void OnLearningDone()
        {
            _learningDone++;
            var(today, month) = FixStatsAndGetCurrent();

            today.LearningDone++;
            month.LearningDone++;
            OnAnyActivity();
        }
        public void UpdateScore(int newScore)
        {
            _totalScore = newScore;
            var(today, month) = FixStatsAndGetCurrent();

            today.TotalScore = newScore;
            month.TotalScore =  newScore;
            
            OnAnyActivity();
        }

    }
 
    public enum UserSource
    {
        Unknown = 0,
        Telegram = 1,
    }
}