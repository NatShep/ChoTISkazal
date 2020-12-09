using System;
using MongoDB.Bson.Serialization.Attributes;

namespace SayWhat.MongoDAL.Users
{
    public class StatsBase
    {
        [BsonElement("w")]
        [BsonDefaultValue(0)]
        [BsonIgnoreIfDefault]
        public int WordsLearnt { get; set; }
        
        [BsonElement("tc")]
        [BsonDefaultValue(0)]
        [BsonIgnoreIfDefault]
        public int TranslationRequests { get; set; }
        [BsonElement("pc")]
        [BsonDefaultValue(0)]
        [BsonIgnoreIfDefault]
        public int NewPairsSelected { get; set; }
        [BsonElement("qp")]
        [BsonDefaultValue(0)]
        [BsonIgnoreIfDefault]
        public int QuestionsPassed { get; set; }
        [BsonElement("qf")]
        [BsonDefaultValue(0)]
        [BsonIgnoreIfDefault]
        public int QuestionsFailed { get; set; }
        [BsonElement("ec")]
        [BsonIgnoreIfDefault]
        public int ExamsPassed { get; set; }
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
            set => Months = (ushort) (value - DayCountStarts).TotalDays;
        } 
    }
}