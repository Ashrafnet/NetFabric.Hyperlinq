using System;
using System.Collections.Generic;

namespace NetFabric.Hyperlinq
{
    public static partial class ReadOnlyCollection
    {
        public static int Count<TEnumerable, TSource>(this TEnumerable source)
            where TEnumerable : IReadOnlyCollection<TSource>
            => source.Count;
    }
}

