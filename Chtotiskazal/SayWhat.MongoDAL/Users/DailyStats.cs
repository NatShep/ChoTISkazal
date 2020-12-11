using System;
using MongoDB.Bson.Serialization.Attributes;
using SayWhat.MongoDAL.Words;

namespace SayWhat.MongoDAL.Users
{
    public class StatsBase
    {
        [BsonElement("sc")] 
        private int[] _scoreChangings;
        
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
        [BsonIgnoreIfDefault]
        private double _leftToA2Changing;
        [BsonElement("sd")] 
        [BsonIgnoreIfDefault]
        private double _absoluteScoreChanging;
        
        [BsonElement("oc")] 
        [BsonIgnoreIfDefault]
        private int _outdatedChanging;

        public int WordsLearnt => Math.Max(0, _a2WordCount) + Math.Max(0, _a3WordCount);
        public WordStatsChanging CummulativeStatsChanging 
            => new WordStatsChanging(
                _scoreChangings,
                _absoluteScoreChanging,
                _leftToA2Changing,
                _outdatedChanging);
        public void AppendStats(WordStatsChanging statsChanging)
        {
            if (_scoreChangings == null)
                _scoreChangings = statsChanging.WordScoreChangings;
            else
                _scoreChangings.AddValuesInplace(statsChanging.WordScoreChangings);
            
            _leftToA2Changing      += statsChanging.LeftToA2Changing;
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