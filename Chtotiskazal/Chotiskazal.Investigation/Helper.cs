using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Chotiskazal.Investigation {

static class Helper {
    public static int Passed(this IEnumerable<Qmodel> a) => (100 * a.Count(a => a.Result)) / a.Count();

    public static void AppendTableItem(this StringBuilder sb, object a) {
        if (a == null)
            sb.Append("| --- ");
        else
            sb.Append("| " + a.ToString().PadLeft(3) + " ");
    }

    public static (double rsquared, double yintercept, double slope ) LinearRegression(IList<(double, double)> vals) {
        LinearRegression(
            vals.Select(v => v.Item1).ToArray(), vals.Select(v => v.Item2).ToArray(), 0, vals.Count,
            out var rsquared, out var yintercept, out var slope);
        return (rsquared, yintercept, slope);
    }

    /// <summary>
    /// Fits a line to a collection of (x,y) points.
    /// </summary>
    /// <param name="xVals">The x-axis values.</param>
    /// <param name="yVals">The y-axis values.</param>
    /// <param name="inclusiveStart">The inclusive inclusiveStart index.</param>
    /// <param name="exclusiveEnd">The exclusive exclusiveEnd index.</param>
    /// <param name="rsquared">The r^2 value of the line.</param>
    /// <param name="yintercept">The y-intercept value of the line (i.e. y = ax + b, yintercept is b).</param>
    /// <param name="slope">The slop of the line (i.e. y = ax + b, slope is a).</param>
    public static void LinearRegression(
        double[] xVals, double[] yVals,
        int inclusiveStart, int exclusiveEnd,
        out double rsquared, out double yintercept,
        out double slope) {
        Debug.Assert(xVals.Length == yVals.Length);
        double sumOfX = 0;
        double sumOfY = 0;
        double sumOfXSq = 0;
        double sumOfYSq = 0;
        double ssX = 0;
        double ssY = 0;
        double sumCodeviates = 0;
        double sCo = 0;
        double count = exclusiveEnd - inclusiveStart;

        for (int ctr = inclusiveStart; ctr < exclusiveEnd; ctr++)
        {
            double x = xVals[ctr];
            double y = yVals[ctr];
            sumCodeviates += x * y;
            sumOfX += x;
            sumOfY += y;
            sumOfXSq += x * x;
            sumOfYSq += y * y;
        }

        ssX = sumOfXSq - ((sumOfX * sumOfX) / count);
        ssY = sumOfYSq - ((sumOfY * sumOfY) / count);
        double RNumerator = (count * sumCodeviates) - (sumOfX * sumOfY);
        double RDenom = (count * sumOfXSq - (sumOfX * sumOfX)) * (count * sumOfYSq - (sumOfY * sumOfY));
        sCo = sumCodeviates - ((sumOfX * sumOfY) / count);

        double meanX = sumOfX / count;
        double meanY = sumOfY / count;
        double dblR = RNumerator / Math.Sqrt(RDenom);
        rsquared = dblR * dblR;
        yintercept = meanY - ((sCo / ssX) * meanX);
        slope = sCo / ssX;
        
    }
}

}