using System;
using System.Collections.Generic;
using System.Linq;

namespace SayWhat.Bll
{
    public static class RandomTools
    {
        public static readonly Random Rnd = new Random(DateTime.Now.Millisecond);
        public static T GetRandomItem<T>(this IList<T> origin)
        {
            var rnd = Rnd.Next(origin.Count - 1);
            return origin[rnd];
        }
        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> origin)
        {
            var list = origin.ToList();
            while (list.Count>0)
            {
                var i = Rnd.Next(list.Count);
                var item = list[i];
                list.RemoveAt(i);
                yield return item;
            }
        }

        public static double RandomNormal(double mean, double stdDev)
        {
            var u1 = 1.0 - Rnd.NextDouble(); //uniform(0,1] random doubles
            var u2 = 1.0 - Rnd.NextDouble();
            var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                                Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            var randNormal =
                mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
            return randNormal;
        }
    }
}
