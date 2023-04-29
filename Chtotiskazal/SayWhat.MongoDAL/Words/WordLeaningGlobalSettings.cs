namespace SayWhat.MongoDAL.Words
{
    public static class WordLeaningGlobalSettings{
        
        /// <summary>
        /// Absolute score reduced by {AgingFactor} per day in AgedScore calculation
        /// </summary>
        public const double AgingFactor = 0.33;
        //probability reduces by reducingPerPointFactor for every res point
        
        /// <summary>
        /// Order index (probability of word appearing in exam) reduces
        /// for {ReducingPerPointFactor} times for each AgedScore
        /// </summary>
        public const double ReducingPerDayPowFactor = 1.7;
        public const double ReduceRateWhenQuestionFailed = 0.7;
        public const double ScoresForPassedQuestion = 0.4;
        public const double WellDoneWordMinScore = 8.0;
        public const double LearnedWordMinScore = 4.0;
        public const double IncompleteWordMinScore = 2.66;
        public const double FamiliarWordMinScore = 1.6;

        //scores:
        public static double NewWordGamingScore { get; set; } = 4;
        public static double NewPairGamingScore { get; set; } = 0.1;
        public static double QuestionPassedGamingScore { get; set; } = 1;
        public static double QuestionFailedGamingScore { get; set; } = -3;
        public static double LearningDoneGamingScore { get; set; } = 0;
    }
}