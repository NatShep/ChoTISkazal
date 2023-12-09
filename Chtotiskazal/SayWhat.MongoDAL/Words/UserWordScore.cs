using System;
using System.ComponentModel;

namespace SayWhat.MongoDAL.Words;

/// <summary>
/// current word score
/// </summary>
public class UserWordScore
{
    private readonly DateTime? _lastAskTime;
    public static UserWordScore Zero => new UserWordScore(0, DateTime.Now);
    public double AbsoluteScore { get; }
    public bool IsOutdated => IsLearned &&
                              AgedScore < WordLeaningGlobalSettings.LearningWordMinScore;
    private bool IsLearned => AbsoluteScore > WordLeaningGlobalSettings.WellDoneWordMinScore;
        
    //res reduces for 1 point per AgingFactor days
    public double AgedScore
    {
        get
        {
            //if there were no asked question yet - return AbsoluteScore, as lowest possible probability  
            if (_lastAskTime == null || (DateTime.Now - _lastAskTime.Value).TotalDays == 0)
                return AbsoluteScore;

            var newAgedScore = Math.Max(0, AbsoluteScore - 
                                           (DateTime.Now - _lastAskTime.Value).TotalDays * WordLeaningGlobalSettings.AgingFactor);
            return newAgedScore;
        }
    }

    public UserWordScore(double absoluteScore, DateTime? lastAskTime)
    {
        AbsoluteScore = absoluteScore;
        _lastAskTime = lastAskTime;
    }

    public static WordStatsChanging operator - (UserWordScore laterScore,UserWordScore earlierScore)
    {
        var scores = new int[8];
        scores[WordStatsChanging.CategorizedScore(earlierScore.AbsoluteScore)]--;
        scores[WordStatsChanging.CategorizedScore(laterScore.AbsoluteScore)]++;
            
        /*
        int outdatedChanging = 0;
        var agedScoreBefore = earlierScore.AgedScore;
        var agedScoreAfter  = laterScore.AgedScore;
        if (agedScoreBefore < WordLeaningGlobalSettings.LearnedWordMinScore)
            outdatedChanging--;
        if (agedScoreAfter < WordLeaningGlobalSettings.LearnedWordMinScore)
            outdatedChanging++;
        */
            
        return new WordStatsChanging(
            scores,
            laterScore.AbsoluteScore - earlierScore.AbsoluteScore);
        // laterScore.AbsoluteScore - earlierScore.AbsoluteScore);
    }
}