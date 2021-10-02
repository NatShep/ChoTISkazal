using System.Collections.Generic;
using System.Linq;

namespace Chotiskazal.Investigation
{
    static class Helper
    {
        public static int Passed(this IEnumerable<Qmodel> a) => (100 * a.Count(a => a.Result)) / a.Count();
    }
}