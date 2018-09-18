using System.Collections.Generic;
using System.Linq;

namespace ComponentAsService
{
    public static class EnumerableNotContainExtensions
    {
        /// <returns>True iff  <paramref name="list"/>.Contains( <paramref name="value" /> )</returns>
        public static bool IsInList<T>(this T value, IEnumerable<T> list) => list.Contains(value);
        
        /// <returns>True iff  <paramref name="list"/>.DoesNotContain( <paramref name="value" /> )</returns>
        public static bool IsNotInList<T>(this T value, IEnumerable<T> list) => !list.Contains(value);
                
        /// <returns>True iff  <paramref name="list"/>.DoesNotContain( <paramref name="value" /> )</returns>
        public static bool DoesNotContain<T>(this IEnumerable<T> list, T value) => !list.Contains(value);
    }
}
