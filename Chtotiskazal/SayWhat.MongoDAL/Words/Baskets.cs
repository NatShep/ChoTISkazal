using System;
using System.Collections.Generic;

namespace SayWhat.MongoDAL.Words;

public static class Baskets
{
    public const int MaxObservingBasketNumber = 7;
    
    public static int[] CreateBaskets() => new int[MaxObservingBasketNumber + 1];

    public static int BasketCountOf(this IReadOnlyList<int> baskets, int minScore, int? maxScore = null)
    {
        if (baskets == null)
            return 0;
        var maxCategory = maxScore == null
            ? baskets.Count
            : Math.Min(baskets.Count, ScoreToBasketNumber(maxScore.Value));
        var minCategory = ScoreToBasketNumber(minScore);
        var acc = 0;
        for (int i = minCategory; i < maxCategory; i++)
        {
            acc += baskets[i];
        }

        return acc;
    }

    /// <summary>
    /// Shows basket number for given absolute score
    /// </summary>
    public static int ScoreToBasketNumber(double absoluteScore)
    {
        int normalizedAbsScore = (int)absoluteScore / 2;
        return normalizedAbsScore >= MaxObservingBasketNumber ? MaxObservingBasketNumber : normalizedAbsScore;
    }
}