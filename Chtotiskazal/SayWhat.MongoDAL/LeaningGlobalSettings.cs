using System.Linq;

namespace SayWhat.MongoDAL;

public static class WordLeaningGlobalSettings {
    /// <summary>
    /// Minimum = 1. Addition to Delta between score of question, and question hardness
    /// The highter it is - the more random question appears for target words
    /// the lower it is - the more expected, that only closest to word by hardness question appears  
    /// </summary>
    public const double TheDampingFactorOfTheChoiceOfQuestions = 4.5;
    /// <summary>
    /// Absolute score reduced by {AgingFactor} per day in AgedScore calculation
    /// </summary>
    public const double AgingFactor = 0.33;
    //probability reduces by reducingPerPointFactor for every res point

    public const double AverageScoresForFailedQuestion = 0.4;

    public const double AverageScoresForPassedQuestion = 0.4;

    public const double StartScoreForWord = 0;
    public const double LearningWordMinScore = 3.5;
    public const double WellDoneWordMinScore = 10.0;
    public const double LearnedWordMinScore = 14.0;
    public const double WellLearnedWordAbsScore = 16.0;
        
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
        Enumerable.Range((int)LearnedWordMinScore, (int)WellLearnedWordAbsScore).ToArray();
}