using System;
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
        [BsonElement("sc")]     private int[] _countByCategoryScores;
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
        
        [BsonElement("oc")] 
        private int _outdatedWordsCount;
        [BsonElement("gc")]
        private int _gamingScore;
        #endregion

         public DateTime LastActivity => _lastActivity;

         public int OutdatedWordsCount => _outdatedWordsCount;
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
         public int WordsLearned => CountOf((int)WordLeaningGlobalSettings.LearnedWordMinScore,10);
         public Zen Zen => new Zen(_countByCategoryScores,_outdatedWordsCount);
         public int GamingScore => _gamingScore;

         public int CountOf(int minScoreCategory, int maxScoreCategory)
         {
             if (_countByCategoryScores == null)
                 return 0;
             int acc = 0;
             for (int i = minScoreCategory; i < maxScoreCategory && i<_countByCategoryScores.Length; i++)
             {
                 acc+= _countByCategoryScores[i];
             }

             return acc;
         }
         
         public void OnAnyActivity()
         {
            _lastActivity = DateTime.Now;
         }
        
        public int OnNewWordAdded(WordStatsChanging statsChanging, int pairsCount, int examplesCount)
        {
            _wordsCount++;
            var(today, month) = FixStatsAndGetCurrent();

            today.WordsAdded++;
            month.WordsAdded++;

            var gamingScore = WordLeaningGlobalSettings.NewWordGamingScore * Zen.AddWordsBonusRate;
            today.OnGameScoreIncreased(gamingScore);
            month.OnGameScoreIncreased(gamingScore);
            _gamingScore += gamingScore;
            
            return gamingScore + OnPairsAdded(
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
                while ((today - LastDaysStats[0].Date).TotalDays>49)
                {
                    LastDaysStats.RemoveAt(0);
                }
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

        public int OnPairsAdded(WordStatsChanging statsChanging, int pairsCount, int examplesCount)
        {
            _pairsCount    += pairsCount;
            _examplesCount += examplesCount;
            
            var (today, month) = FixStatsAndGetCurrent();
            today.PairsAdded+= pairsCount;
            today.ExamplesAdded+= examplesCount;
            month.PairsAdded+= pairsCount;
            month.ExamplesAdded+= examplesCount;
            var gamingScore =pairsCount * WordLeaningGlobalSettings.NewPairGamingScore* Zen.AddWordsBonusRate ;
            ApplyWordStatsChangings(statsChanging, today, month, gamingScore);
            OnAnyActivity();
            return gamingScore;
        }

        public void OnStatsChangings(WordStatsChanging changing)
        {
            var (today, month) = FixStatsAndGetCurrent();
            ApplyWordStatsChangings(changing, today, month,0);
        }

    
        
        private void ApplyWordStatsChangings(WordStatsChanging statsChanging, DailyStats dailyStats, MonthsStats monthsStats, int gamingScore)
        {
            dailyStats.AppendStats(statsChanging);
            monthsStats.AppendStats(statsChanging);
            dailyStats.OnGameScoreIncreased(gamingScore);
            monthsStats.OnGameScoreIncreased(gamingScore);
            _gamingScore += gamingScore;
            if (_gamingScore <= 0) _gamingScore = 0;
            
            if (_countByCategoryScores == null)
                _countByCategoryScores = statsChanging.WordScoreChangings;
            else
                _countByCategoryScores.AddValuesInplace(statsChanging.WordScoreChangings);
            _countByCategoryScores.SetLowLimitInplace(0);
            
            _outdatedWordsCount    += statsChanging.OutdatedChanging;
            
            if (_outdatedWordsCount < 0)    _outdatedWordsCount = 0;
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
        public int OnQuestionPassed(WordStatsChanging statsChanging)
        {
            _questionPassed++;
            var(today, month) = FixStatsAndGetCurrent();

            today.QuestionsPassed++;
            month.QuestionsPassed++;

            OnAnyActivity();

            var gamingScore = WordLeaningGlobalSettings.QuestionPassedGamingScore * Zen.LearnWordsBonusRate;
            ApplyWordStatsChangings(statsChanging, today, month, gamingScore);
            return gamingScore;
        }
        public int OnQuestionFailed(WordStatsChanging statsChanging)
        {
            _questionFailed++;
            var(today, month) = FixStatsAndGetCurrent();

            today.QuestionsFailed++;
            month.QuestionsFailed++;

            OnAnyActivity();

            ApplyWordStatsChangings(statsChanging, today, month, WordLeaningGlobalSettings.QuestionFailedGamingScore);
            return WordLeaningGlobalSettings.QuestionFailedGamingScore;
        }
        public int OnLearningDone()
        {
            _learningDone++;
            var(today, month) = FixStatsAndGetCurrent();

            today.LearningDone++;
            month.LearningDone++;

            var gamingScore = WordLeaningGlobalSettings.LearningDoneGamingScore* Zen.LearnWordsBonusRate;
            ApplyWordStatsChangings(WordStatsChanging.Zero, today, month, gamingScore);
            OnAnyActivity();

            return gamingScore;
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

        public void RecreateStatistic(IEnumerable<UserWordModel> allUserWords)
        {
            if(_wordsCount== _countByCategoryScores?.Sum())
                return;
            
            var days = new Dictionary<DateTime, DailyStats>();
            var months = new Dictionary<DateTime,MonthsStats>();
            _countByCategoryScores = new int[8];
            _outdatedWordsCount = 0;
            this._gamingScore = 0;
            foreach (var word in allUserWords)
            {
                var creationTime = word.Id.CreationTime;
                var day = creationTime.Date;
                if (!days.TryGetValue(day, out var dailyStats))
                {
                    dailyStats = new DailyStats()
                    {
                        Date = day
                    };
                    days.Add(day, dailyStats);
                }

                var month = new DateTime(creationTime.Year, creationTime.Month, 1);
                if (!months.TryGetValue(month, out var monthsStats))
                {
                    monthsStats = new MonthsStats()
                    {
                        Date = month
                    };
                    months.Add(month, monthsStats);
                }

                var eCount = word.Translations.Select(t => t.Examples.Length).Sum();
                monthsStats.ExamplesAdded += eCount;
                dailyStats.ExamplesAdded += eCount;
                monthsStats.PairsAdded += word.Translations.Length;
                dailyStats.PairsAdded += word.Translations.Length;
                monthsStats.WordsAdded++;
                dailyStats.WordsAdded++;

                dailyStats.QuestionsPassed += word.QuestionPassed;
                monthsStats.QuestionsPassed += word.QuestionPassed;
                dailyStats.QuestionsFailed += word.QuestionAsked - word.QuestionPassed;
                monthsStats.QuestionsFailed += word.QuestionAsked - word.QuestionPassed;

                var wc = WordStatsChanging.CreateForNewWord(word.AbsoluteScore);

                var score = WordLeaningGlobalSettings.NewWordGamingScore +
                            WordLeaningGlobalSettings.NewPairGamingScore * word.Translations.Length +
                            WordLeaningGlobalSettings.ScoresForPassedQuestion * word.QuestionPassed +
                            WordLeaningGlobalSettings.QuestionFailedGamingScore *
                            (word.QuestionAsked - word.QuestionPassed);
                ApplyWordStatsChangings(wc, dailyStats, monthsStats,(int)score);
            }
            if(LastDaysStats==null)
                LastDaysStats = new List<DailyStats>();
            else
                LastDaysStats.Clear();
            
            if(LastMonthStats==null)
                LastMonthStats = new List<MonthsStats>();
            else
                LastMonthStats.Clear();

            foreach (var day in days) {
                LastDaysStats.Add(day.Value);
            }
            LastDaysStats.Sort((d1,d2)=>d1.Day.CompareTo(d2.Day));
            
            foreach (var month in months) {
                LastMonthStats.Add(month.Value);
            }
            LastMonthStats.Sort((d1,d2)=>d1.Months.CompareTo(d2.Months));
            _wordsCount = allUserWords.Count();
        }
    }
}