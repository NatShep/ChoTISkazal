using System.Linq;
using System;
using System.Collections.Generic;

namespace SayWhat.MongoDAL;

public static class Rand {
    public static readonly Random Rnd = new Random(DateTime.Now.Millisecond);

    public static T GetRandomItemOrNull<T>(this IEnumerable<T> origin) => origin.ToList().GetRandomItemOrNull();

    public static T GetRandomItemOrNull<T>(this IList<T> origin) {
        if (origin.Count == 0)
            return default;
        var rnd = Rnd.Next(origin.Count);
        return origin[rnd];
    }

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> origin) {
        var list = origin.ToList();
        while (list.Count > 0) {
            var i = Rnd.Next(list.Count);
            var item = list[i];
            list.RemoveAt(i);
            yield return item;
        }
    }

    public static double RandomNormal(double mean, double stdDev) {
        var u1 = 1.0 - Rnd.NextDouble(); //uniform(0,1] random doubles
        var u2 = 1.0 - Rnd.NextDouble();
        var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                            Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
        var randNormal =
            mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
        return randNormal;
    }

    public static int RandomIn(int inclusiveMin, int inclusiveMax)
        => Rnd.Next(inclusiveMin, inclusiveMax + 1);

    public static int UpTo(in int inclusiveMax)
        => Rnd.Next(inclusiveMax + 1);

    public static int Next()
        => Rnd.Next();

    public static double NextDouble()
        => Rnd.NextDouble();
}