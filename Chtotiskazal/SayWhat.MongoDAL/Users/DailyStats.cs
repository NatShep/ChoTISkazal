using System;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;
using SayWhat.MongoDAL.Words;

namespace SayWhat.MongoDAL.Users;

public class StatsBase
{
    [BsonElement("sc")] private int[] _scoreChangesBaskets;

    [BsonElement("sd")] [BsonIgnoreIfDefault]
    private double _absoluteScoreChanging;

    [BsonElement("gs")] [BsonIgnoreIfDefault]
    private double _gameScoreChanging;

    public int CountOf(int minScore, int? maxScore = null)
        => CummulativeStatsChange.CountOf(minScore, maxScore);

    public void OnGameScoreIncreased(double gameScoreChanging)
    {
        _gameScoreChanging += gameScoreChanging;
    }

    public int WordsLearnt => CountOf((int)WordLeaningGlobalSettings.WellDoneWordMinScore);

    public WordStatsChange CummulativeStatsChange
        => new WordStatsChange(
            _scoreChangesBaskets,
            _absoluteScoreChanging);

    public void AppendStats(WordStatsChange statsChange)
    {
        if (_scoreChangesBaskets == null)
            _scoreChangesBaskets = statsChange.Baskets.ToArray();
        else
            _scoreChangesBaskets.AddValuesInplace(statsChange.Baskets);

        _absoluteScoreChanging += statsChange.AbsoluteScoreChange;
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

    public double GameScoreChanging => _gameScoreChanging;
}

[BsonIgnoreExtraElements]
public class DailyStats : StatsBase
{
    [BsonElement("d")] [BsonRequired] public ushort Day { get; set; }

    private static readonly DateTime DayCountStarts = new DateTime(2020, 1, 1);

    [BsonIgnore]
    public DateTime Date
    {
        get => DayCountStarts.AddDays(Day);
        set => Day = (ushort)(value - DayCountStarts).TotalDays;
    }
}

[BsonIgnoreExtraElements]
public class MonthsStats : StatsBase
{
    [BsonElement("m")] [BsonRequired] public ushort Months { get; set; }

    private static readonly DateTime DayCountStarts = new DateTime(2020, 1, 1);

    [BsonIgnore]
    public DateTime Date
    {
        get => DayCountStarts.AddMonths(Months);
        set => Months = (ushort)MonthDifference(value, DayCountStarts);
    }

    private static int MonthDifference(DateTime lValue, DateTime rValue)
        => (lValue.Month - rValue.Month) + 12 * (lValue.Year - rValue.Year);
}