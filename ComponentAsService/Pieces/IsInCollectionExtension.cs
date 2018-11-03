using System.Collections.Generic;
using System.Linq;

namespace Component.As.Service.Pieces
{
    static class IsInCollectionExtension
    {
        public static bool IsIn<T>(this T @this, IEnumerable<T> collection) => collection.Contains(@this);
    }
}