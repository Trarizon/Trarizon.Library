using System.Collections;

namespace Trarizon.Library.Linq;

public static partial class TraEnumerable
{
    internal static IEnumerator<T> EmptyEnumerator<T>() => EmptyEnumeratorImpl<T>.Instance;

    private static bool TryIterate<T>(this IEnumerator<T> enumerator, int count, out int iteratedCount)
    {
        iteratedCount = 0;
        if (count <= 0)
            return true;

        while (enumerator.MoveNext()) {
            if (++iteratedCount >= count)
                return true;
        }
        return false;
    }

    private sealed class EmptyEnumeratorImpl<T> : IEnumerator<T>
    {
        public static readonly EmptyEnumeratorImpl<T> Instance = new();

        private EmptyEnumeratorImpl() { }

        public T Current => default!;

        object? IEnumerator.Current => Current;

        public void Dispose() { }
        public bool MoveNext() => false;
        public void Reset() { }
    }
}
