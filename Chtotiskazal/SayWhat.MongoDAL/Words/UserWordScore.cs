using System;

namespace SayWhat.MongoDAL.Words
{
    /// <summary>
    /// current word score
    /// </summary>
    public class UserWordScore
    {
        private readonly DateTime? _lastAskTime;
        public static UserWordScore Zero => new UserWordScore(0, DateTime.Now);
        public double AbsoluteScore { get; }

        public bool IsOutdated => IsLearned &&
                                  AgedScore < WordLeaningGlobalSettings.LearnedWordMinScore;
        public bool IsLearned => AbsoluteScore >= WordLeaningGlobalSettings.LearnedWordMinScore;
        
        //res reduces for 1 point per AgingFactor days
        public double AgedScore
        {
            get
            {
                //if there were no asked question yet - return 0, as lowest possible probability  
                if (_lastAskTime == null) return 0;
                return Math.Max(0, AbsoluteScore - (DateTime.Now - _lastAskTime.Value).TotalDays
                    / WordLeaningGlobalSettings.AgingFactor);
            }
        }

        public UserWordScore(double absoluteScore, DateTime? lastAskTime)
        {
            AbsoluteScore = absoluteScore;
            _lastAskTime = lastAskTime;
        }

        public static WordStatsChanging operator - (UserWordScore laterScore,UserWordScore earlierScore)
        {
            int a0 = 0;
            int a1 = 0;
            int a2 = 0;
            int a3 = 0;
            
            if (earlierScore.AbsoluteScore >= WordStatsChanging.A3LearnScore)      a3--;
            else if (earlierScore.AbsoluteScore >= WordLeaningGlobalSettings.LearnedWordMinScore) a2--;
            else if (earlierScore.AbsoluteScore >= WordStatsChanging.A1LearnScore) a1--;
            else a0--;
            
            if (laterScore.AbsoluteScore >= WordStatsChanging.A3LearnScore)      a3++;
            else if (laterScore.AbsoluteScore >= WordLeaningGlobalSettings.LearnedWordMinScore) a2++;
            else if (laterScore.AbsoluteScore >= WordStatsChanging.A1LearnScore) a1++;
            else a0++;

            var originA2Score = Math.Min(WordLeaningGlobalSettings.LearnedWordMinScore, earlierScore.AbsoluteScore);
            var resultA2Score = Math.Min(WordLeaningGlobalSettings.LearnedWordMinScore, laterScore.AbsoluteScore);

            int outdatedChanging = 0;
            var agedScoreBefore = earlierScore.AgedScore;
            var agedScoreAfter  = laterScore.AgedScore;
            if (agedScoreBefore < WordLeaningGlobalSettings.LearnedWordMinScore)
                outdatedChanging--;
            if (agedScoreAfter < WordLeaningGlobalSettings.LearnedWordMinScore)
                outdatedChanging++;

            
            return new WordStatsChanging(
                a0: a0,
                a1: a1,
                a2: a2,
                a3: a3,
                absoluteScoreChanging: laterScore.AbsoluteScore - earlierScore.AbsoluteScore,
                leftLeftToA2: resultA2Score - originA2Score, 
                outdatedChanging: outdatedChanging);
        }
    }
}