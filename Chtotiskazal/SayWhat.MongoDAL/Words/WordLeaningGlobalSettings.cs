namespace SayWhat.MongoDAL.Words
{
    public static class WordLeaningGlobalSettings{
        
        /// <summary>
        /// If question failed - Absolute score reduced to {PenaltyScore} 
        /// </summary>
        public const int PenaltyScore = 9;
        /// <summary>
        /// Absolute score reduced by {AgingFactor} per day in AgedScore calculation
        /// </summary>
        public const double AgingFactor = 1;
        //probability reduces by reducingPerPointFactor for every res point
        
        /// <summary>
        /// Order index (probability of word appearing in exam) reduces
        /// for {ReducingPerPointFactor} times for each AgedScore
        /// </summary>
        public const double ReducingPerPointFactor = 1.7;

        public const double ReduceRateWhenQuestionFailed = 0.7;
        public const double ScoresForPassedQuestion = 1.0;
        public const double WellDoneWordMinScore = 24.0;
        public const double LearnedWordMinScore = 12.0;
        public const double FamiliarWordMinScore = 4.0;
        public const double IncompleteWordMinScore = 7.0;
    }
}