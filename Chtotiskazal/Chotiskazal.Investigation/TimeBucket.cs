using System;
using System.Diagnostics;
using System.Text;

namespace Chotiskazal.Investigation {
class TimeBucket {
    public readonly int SecLow;
    public readonly int SecHi;

    public int Count { get; private set; }
    public int Passed { get; private set; }
    public int[] CountByScore = new int[15];
    public int[] PassedByScore = new int[15];
    
    

    public void Put(double score, bool result) {
        Count++;

        var item = (int)score;
        if (item >= CountByScore.Length)
            item = CountByScore.Length - 1;

        CountByScore[item]++;
        if (result)
        {
            Passed++;
            PassedByScore[item]++;
        }
    }

    public TimeBucket(int secLow, int secHi) {
        SecLow = secLow;
        SecHi = secHi;
    }

    public override string ToString() {
        if (Count == 0)
            return TimeSpan.FromSeconds(SecLow) + " - " + TimeSpan.FromSeconds(SecHi) + ":   NA";

        var sb = new StringBuilder(
            $"{(SecHi-SecLow)/2:0000}) " +
             TimeSpan.FromSeconds(SecLow) +
            " - " +
            TimeSpan.FromSeconds(SecHi) +
            ":   " +
            Passed * 100 / Count +
            $"   {GetWeightedScore():00.0}" +
            "   ");
            
        for (int i = 0; i < CountByScore.Length - 1; i += 2) {
            var relative = Percentage(i, 3);
            if (relative == null)
                sb.Append("| --- ");
            else
                sb.Append($"| {relative:000} ");
        }

        return sb.ToString();
    }
    private double? GetWeightedScore() {
        /*
         * 
у разных очков разный вес
если в корзинке будут только высокие числа то это значит что мы будем переоценивать
значит чем выше тем ниже вес
например у 1 очка - вес 1. У очка 15 - вес 0.5

sum(p[i] * w[i]) //сдано
/
sum(c[i] * w[i]) //кол-во 

k+b = 1
15k+b = 0.5

k = -(0.5/14) = -(5/140)
b = 29/28

3k+b = 1
10k+b = 0.5

-7k = 0.5
-14k = 1
k = -1/14
b = 17/14
         */
        double getWeight(int i) =>
            i switch {
                0 => 0.1,
                1 => 0.3,
                2 => 0.6,
                _ =>  /*(-1 / 14.0) * i + 17 / 14.0 */(-5 / 140.0) * i + 29 / 28.0
            };

        var top = 0.0;
        var bottom = 0.0;
        for (int i = 3; i < 9; i += 1) {
            /*
             * 
sum(p[i] * w[i]) //сдано
/
sum(c[i] * w[i]) //кол-во 

             */

            var w = getWeight(i);
            top += PassedByScore[i] * w;
            bottom += CountByScore[i] * w;
        }
        return top * 100 / bottom;
    }
    private double? Percentage(int i, int threshold) {
        var count = CountByScore[i] + CountByScore[i + 1];
        var score = PassedByScore[i] + PassedByScore[i + 1];
        if (count < threshold)
            return null;
        else
            return score * 100 / count;
    }
}
}