using System.Runtime.InteropServices;
using Trarizon.Library.Collections;

namespace System.Runtime.InteropServices;

#if NETSTANDARD

static class CollectionsMarshal
{
        public static Span<T> AsSpan<T>(List<T> list)
            => TraCollection.UnsafeAccess<T>.GetItems(list).AsSpan(0, list.Count);

}

#endif
