using System;
using MongoDB.Bson.Serialization.Attributes;
using SayWhat.MongoDAL.Words;

namespace SayWhat.MongoDAL.Users
{
    public class StatsBase
    {
        [BsonElement("sc")] 
        private int[] _scoreChangings;
        [BsonElement("sd")] 
        [BsonIgnoreIfDefault]
        private double _absoluteScoreChanging;
        [BsonElement("oc")] 
        [BsonIgnoreIfDefault]
        private int _outdatedChanging;

        public int CountOf(int minLearnCategory, int maxLearnCategory)
        {
            var acc = 0;
            for (int i = minLearnCategory; i < _scoreChangings.Length && i< maxLearnCategory; i++)
            {
                acc += _scoreChangings[i];
            }

            return acc;
        }
        public int WordsLearnt => CountOf((int)WordLeaningGlobalSettings.LearnedWordMinScore,10);
        public WordStatsChanging CummulativeStatsChanging 
            => new WordStatsChanging(
                _scoreChangings,
                _absoluteScoreChanging,
                _outdatedChanging);
        public void AppendStats(WordStatsChanging statsChanging)
        {
            if (_scoreChangings == null)
                _scoreChangings = statsChanging.WordScoreChangings;
            else
                _scoreChangings.AddValuesInplace(statsChanging.WordScoreChangings);
            
            _absoluteScoreChanging += statsChanging.AbsoluteScoreChanging;
            _outdatedChanging      += statsChanging.OutdatedChanging;
        }
        
        [BsonElement("wa")]
        [BsonDefaultValue(0)]
        [BsonIgnoreIfDefault]
        public int WordsAdded { get; set; }

        [BsonElement("ec")]
        [BsonDefaultValue(0)]
        [BsonIgnoreIfDefault]
        public int ExamplesAdded { get; set; }
        [BsonElement("pc")]
        [BsonDefaultValue(0)]
        [BsonIgnoreIfDefault]
        public int PairsAdded { get; set; }
        [BsonElement("qp")]
        [BsonDefaultValue(0)]
        [BsonIgnoreIfDefault]
        public int QuestionsPassed { get; set; }
        [BsonElement("qf")]
        [BsonDefaultValue(0)]
        [BsonIgnoreIfDefault]
        public int QuestionsFailed { get; set; }
        [BsonElement("ld")]
        [BsonIgnoreIfDefault]
        public int LearningDone { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class TotalStats : StatsBase
    {
        
    }
    [BsonIgnoreExtraElements]
    public class DailyStats: StatsBase
    {
        [BsonElement("d")]
        [BsonRequired]
        public ushort Day { get; set; }
        
        private static readonly DateTime DayCountStarts = new DateTime(2020, 1, 1);
        
        [BsonIgnore]
        public DateTime Date
        {
            get => DayCountStarts.AddDays(Day);
            set => Day = (ushort) (value - DayCountStarts).TotalDays;
        }

        
    }
    [BsonIgnoreExtraElements]
    public class MonthsStats : StatsBase
    {
        [BsonElement("m")]
        [BsonRequired]
        public ushort Months { get; set; }
        
        private static readonly DateTime DayCountStarts = new DateTime(2020, 1, 1);
        
        [BsonIgnore]
        public DateTime Date
        {
            get => DayCountStarts.AddMonths(Months);
            set => Months = (ushort) MonthDifference(value,DayCountStarts);
        } 
        private static int MonthDifference(DateTime lValue, DateTime rValue) 
            => (lValue.Month - rValue.Month) + 12 * (lValue.Year - rValue.Year);
    }
}