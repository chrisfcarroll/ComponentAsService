using System.Collections.Generic;
using System.Linq;

namespace ComponentAsService2.UseComponentAsService
{
    public static class IsInCollectionExtension
    {
        public static bool IsIn<T>(this T @this, IEnumerable<T> collection) => collection.Contains(@this);
    }
}