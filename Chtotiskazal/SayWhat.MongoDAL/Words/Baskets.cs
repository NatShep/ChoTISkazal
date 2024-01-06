using System;
using System.Collections.Generic;

namespace SayWhat.MongoDAL.Words;

public static class Baskets
{
    public const int MaxObservingBasketNumber = 7;
        
    public static int[] CreateBaskets() => new int[MaxObservingBasketNumber + 1];
    
    /// <summary>
    /// Отдает сумму элементов в корзинках с очками от minScore, до maxScore включительно
    /// </summary>
    /// <param name="baskets">корзинки. В каждой корзинке лежит два скора. [0] -> 0, 1| [1] -> 2,3... [7] -> 14,15+</param>
    /// <param name="minAbsoluteScore"></param>
    /// <param name="maxAbsoluteScore"></param>
    /// <returns></returns>
    public static int BasketCountOfScores(this IReadOnlyList<int> baskets, int minAbsoluteScore, int? maxAbsoluteScore = null)
    {
        if (baskets == null)
            return 0;
        var maxCategory = maxAbsoluteScore == null
            ? baskets.Count
            : Math.Min(baskets.Count, ScoreToBasketNumber(maxAbsoluteScore.Value));
        var minCategory = ScoreToBasketNumber(minAbsoluteScore);
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