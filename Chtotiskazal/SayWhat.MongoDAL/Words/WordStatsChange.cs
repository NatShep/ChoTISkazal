using System.Collections.Generic;

namespace SayWhat.MongoDAL.Words;

public class WordStatsChange {
    public static readonly WordStatsChange Zero = new();

    public static WordStatsChange CreateForScore(double absoluteScore) {
        var scoreChangesBaskets = Words.Baskets.CreateBaskets();
        scoreChangesBaskets[Words.Baskets.ScoreToBasketNumber(absoluteScore)]++;
        return new WordStatsChange(scoreChangesBaskets, absoluteScore); 
    }

    public static WordStatsChange operator +(WordStatsChange a, WordStatsChange b) {
        if (a == null)
            return b;
        if (b == null)
            return a;
        return new WordStatsChange(
            a.Baskets.Sum(b.Baskets),
            a.AbsoluteScoreChange + b.AbsoluteScoreChange);
    }

    private WordStatsChange() { }

    public WordStatsChange(int[] wordScoreChanges, double absoluteScoreChange)
    {
        Baskets = wordScoreChanges;
        AbsoluteScoreChange = absoluteScoreChange;
    }
    
    /// <summary>
    /// Baskets. Each baskets handles TWO absolute scores
    /// Basket  scores
    /// [0] ->  0,1
    /// [1] ->  2,3
    /// ...
    /// [7] -> 14,15
    /// </summary>
    public IReadOnlyList<int> Baskets { get; }
    
    public double AbsoluteScoreChange { get; }

    public int CountOf(int minScore, int? maxScore = null) => Baskets.BasketCountOf(minScore, maxScore);
}