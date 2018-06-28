using System.Collections.Generic;

namespace YHCJB.Util
{
    public static class EnumerableExtension
    {
        public static string JoinToString<T>(this IEnumerable<T> elems, string seperator = ", ")
        {
            var (result, first) = ("", true);
            foreach (var e in elems)
            {
                var se = e.ToString();
                if (first)
                {
                    result = se;
                    first = false;
                }
                else
                    result += seperator + se;
            }
            return result;
        }
    }
}
