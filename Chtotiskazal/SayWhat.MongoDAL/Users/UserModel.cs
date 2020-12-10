using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SayWhat.MongoDAL.Words;
#pragma warning disable 169

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
        public List<DailyStats> LastDaysStats { get; set; }
        
        [BsonDefaultValue(null)]
        [BsonIgnoreIfDefault]
        [BsonElement("lms")]
        public List<MonthsStats> LastMonthStats { get; set; }
        

        [BsonElement("scr")] private int _totalScore; 
        [BsonElement("ldc")] private int _learningDone; 
        [BsonElement("qpc")] private int _questionPassed; 
        [BsonElement("qfc")] private int _questionFailed; 
        
        [BsonElement("a0c")] 
        [BsonIgnoreIfDefault]
        private int _a0WordCount;
        [BsonElement("a1c")]
        [BsonIgnoreIfDefault]
        private int _a1WordCount;
        [BsonElement("a2c")] 
        [BsonIgnoreIfDefault]
        private int _a2WordCount;
        [BsonElement("a3c")] 
        [BsonIgnoreIfDefault]
        private int _a3WordCount;
        [BsonElement("l2a2c")] 
        private double _leftToA2;
        
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
         public int WordsLearned => _a2WordCount + _a3WordCount;
            
         public int A0WordCount => _a0WordCount;
         public int A1WordCount => _a1WordCount;
         public int A2WordCount => _a2WordCount;
         public int A3WordCount => _a3WordCount;

         public double LeftToA2 => _leftToA2;


         public void OnAnyActivity()
         {
            _lastActivity = DateTime.Now;
         }
        
        public void OnNewWordAdded(WordStatsChanging statsChanging, int pairsCount, int examplesCount)
        {
            _wordsCount++;
            var(today, month) = FixStatsAndGetCurrent();

            today.WordsAdded++;
            month.WordsAdded++;
            
            OnPairsAdded(
                statsChanging:     statsChanging, 
                pairsCount:        pairsCount, 
                examplesCount:     examplesCount);
        }

        private (DailyStats, MonthsStats) FixStatsAndGetCurrent()
        {
            var daily = GetToday();

            var monthly = GetLastMonth();

            return (daily, monthly);
        }

        public MonthsStats GetLastMonth()
        {
            MonthsStats monthly;
            var today = DateTime.Today;

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

            return monthly;
        }

        public DailyStats GetToday()
        {
            var today = DateTime.Today;
            DailyStats daily;
            if (LastDaysStats == null || LastDaysStats.Count == 0)
            {
                daily = new DailyStats {Date = today};
                LastDaysStats = new List<DailyStats>();
                LastDaysStats.Add(daily);
            }
            else
            {
                var oldest = LastDaysStats[0];
                var oldestDelta = (today - oldest.Date);
                if (oldestDelta.TotalDays > 30)
                    LastDaysStats.RemoveAt(0);
                var lastDay = LastDaysStats.Last();
                if (lastDay.Date != today)
                {
                    daily = new DailyStats {Date = today};
                    LastDaysStats.Add(daily);
                }
                else
                    daily = lastDay;
            }

            return daily;
        }

        public void OnPairsAdded(WordStatsChanging statsChanging, int pairsCount, int examplesCount)
        {
            _pairsCount    += pairsCount;
            _examplesCount += examplesCount;
            
            var (today, month) = FixStatsAndGetCurrent();
            today.PairsAdded+= pairsCount;
            today.ExamplesAdded+= examplesCount;
            month.PairsAdded+= pairsCount;
            month.ExamplesAdded+= examplesCount;
            
            AppendChangingsToStats(statsChanging, today, month);
            OnAnyActivity();
        }

        private void AppendChangingsToStats(WordStatsChanging statsChanging, DailyStats dailyStats, MonthsStats monthsStats)
        {
            dailyStats.AppendStats(statsChanging);
            monthsStats.AppendStats(statsChanging);
            _a0WordCount += statsChanging.A0WordsCountChanging;
            _a1WordCount += statsChanging.A1WordsCountChanging;
            _a2WordCount += statsChanging.A2WordsCountChanging;
            _a3WordCount += statsChanging.A3WordsCountChanging;
            _leftToA2    += statsChanging.LeftToA2Changing;

            if (_a0WordCount < 0) _a0WordCount = 0;
            if (_a1WordCount < 0) _a1WordCount = 0;
            if (_a2WordCount < 0) _a2WordCount = 0;
            if (_a3WordCount < 0) _a3WordCount = 0;

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
        public void OnQuestionPassed(WordStatsChanging statsChanging)
        {
            _questionPassed++;
            var(today, month) = FixStatsAndGetCurrent();

            today.QuestionsPassed++;
            month.QuestionsPassed++;
            AppendChangingsToStats(statsChanging, today, month);
            OnAnyActivity();
        }
        public void OnQuestionFailed(WordStatsChanging statsChanging)
        {
            _questionFailed++;
            var(today, month) = FixStatsAndGetCurrent();

            today.QuestionsFailed++;
            month.QuestionsFailed++;
            
            AppendChangingsToStats(statsChanging, today, month);
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

        
        public IReadOnlyList<DailyStats> GetLastWeek()
        {
            var today = GetToday();
            var dailyStats = new List<DailyStats>(7);
            dailyStats.Add(today);
            for (int i = 1; i < LastDaysStats.Count-1; i++)
            {
                var stats = LastDaysStats[^i];
                if ((today.Date - stats.Date).TotalDays>7)
                    break;
                dailyStats.Add(stats);
            }
            return dailyStats;
        }
        
    }
}