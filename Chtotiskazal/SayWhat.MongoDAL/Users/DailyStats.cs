using System;
using MongoDB.Bson.Serialization.Attributes;
using SayWhat.MongoDAL.Words;

namespace SayWhat.MongoDAL.Users
{
    public class StatsBase
    {
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
        public WordStatsChanging CummulativeStatsChanging 
            => new WordStatsChanging(_a0WordCount, _a1WordCount, _a2WordCount, _a3WordCount, _leftToA2);
        public void AppendStats(WordStatsChanging statsChanging)
        {
            _a0WordCount += statsChanging.A0WordsCountChanging;
            _a1WordCount += statsChanging.A1WordsCountChanging;
            _a2WordCount += statsChanging.A2WordsCountChanging;
            _a3WordCount += statsChanging.A3WordsCountChanging;
            _leftToA2+= statsChanging.LeftToA2Changing;
        }
        
        [BsonElement("wa")]
        [BsonDefaultValue(0)]
        [BsonIgnoreIfDefault]
        public int WordsAdded { get; set; }

        [BsonElement("ec")]
        [BsonDefaultValue(0)]
        [BsonIgnoreIfDefault]
        public int ExamplesAdded { get; set; }

        [BsonElement("w")]
        [BsonDefaultValue(0)]
        [BsonIgnoreIfDefault]
        public int WordsLearnt { get; set; }
        
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
        
        [BsonElement("s")]
        [BsonIgnoreIfDefault]
        public int TotalScore { get; set; }
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