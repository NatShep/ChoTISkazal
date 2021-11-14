using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chotiskazal.Investigation
{
    static class Helper
    {
        public static int Passed(this IEnumerable<Qmodel> a) => (100 * a.Count(a => a.Result)) / a.Count();
        public static void AppendTableItem(this StringBuilder sb, object a) {
            if (a == null)
                sb.Append("| --- ");
            else
                sb.Append("| " + a.ToString().PadLeft(3) + " ");
        }

    }
}