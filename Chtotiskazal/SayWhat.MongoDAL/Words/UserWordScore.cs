using System;

namespace SayWhat.MongoDAL.Words
{
    /// <summary>
    /// current word score
    /// </summary>
    public class UserWordScore
    {
        private readonly double _absoluteRate;
        private readonly DateTime _lastAskTime;
        public static UserWordScore Zero => new UserWordScore(0, DateTime.Now); 
        public UserWordScore(double absoluteRate, DateTime lastAskTime)
        {
            _absoluteRate = absoluteRate;
            _lastAskTime = lastAskTime;
        }

        public static WordStatsChanging operator -(UserWordScore laterScore,UserWordScore earlierScore) 
            => WordStatsChanging.Create(earlierScore._absoluteRate, laterScore._absoluteRate);
    }
}