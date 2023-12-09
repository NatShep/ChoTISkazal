using System.Linq;

namespace SayWhat.MongoDAL.Words;

public static class WordLeaningGlobalSettings {
    /// <summary>
    /// Absolute score reduced by {AgingFactor} per day in AgedScore calculation
    /// </summary>
    public const double AgingFactor = 0.33;
    //probability reduces by reducingPerPointFactor for every res point

    /// <summary>
    /// Order index (probability of word appearing in exam) reduces
    /// for {ReducingPerPointFactor} times for each AgedScore
    /// </summary>
    public const double AverageScoresForFailedQuestion = 0.4;

    public const double AverageScoresForPassedQuestion = 0.4;

    //todo переделать подбор экзаменов и выбор слов по НОВЫМ статам
    public const double StartScoreForWord = 0;
    public const double LearningWordMinScore = 4.0;
    public const double WellDoneWordMinScore = 10.0;
    public const double LearnedWordMinScore = 14.0;
    public const double MaxWordAbsScore = 16;

    //scores:
    public static double NewWordGamingScore { get; set; } = 4;
    public static double NewPairGamingScore { get; set; } = 0.1;
    public static double QuestionPassedGamingScore { get; set; } = 1;
    public static double QuestionFailedGamingScore { get; set; } = -3;
    public static double LearningDoneGamingScore { get; set; } = 0;

    //LearningGroupsByScore
    public static int[] FamiliarWordsScoreRange = Enumerable.Range(0, (int)LearningWordMinScore).ToArray();

    public static int[] LearningWordsScoreRange =
        Enumerable.Range((int)LearningWordMinScore, (int)WellDoneWordMinScore).ToArray();

    public static int[] WellDoneWordScoreRange =
        Enumerable.Range((int)WellDoneWordMinScore, (int)LearnedWordMinScore).ToArray();

    public static int[] LearnedWordScoreRange =
        Enumerable.Range((int)LearnedWordMinScore, (int)MaxWordAbsScore).ToArray();
}