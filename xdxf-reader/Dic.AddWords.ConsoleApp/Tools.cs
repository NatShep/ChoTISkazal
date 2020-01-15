using System;
using System.Collections.Generic;
using System.Linq;

namespace Dic.AddWords.ConsoleApp
{
    public static class Tools
    {
        public static readonly Random Rnd = new Random(DateTime.Now.Millisecond);
        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> origin)
        {
            var list = origin.ToList();
            while (list.Count>0)
            {
                var i = Rnd.Next(list.Count - 1);
                var item = list[i];
                list.RemoveAt(i);
                yield return item;
            }
        }
    }
}
