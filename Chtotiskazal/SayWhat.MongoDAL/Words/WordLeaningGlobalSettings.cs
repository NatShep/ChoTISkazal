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
    }
}