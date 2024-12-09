using System.ComponentModel;

namespace Trarizon.Library.Collections;
partial class TraIter
{
    /// <summary>
    /// Reverse. If <paramref name="source"/> is an <c>IList&lt;T></c>, this method iterates
    /// the collection based on index, as LinQ always create a cache array. So this may perform
    /// better in some case that you just want to iterate a list directly
    /// </summary>
    public static ReverseIterator<T> IterReverse<T>(this IEnumerable<T> source)
    {
        if (source is IList<T> ilist) {
            return new ReverseIterator<T>(ilist);
        }
        else if (source.TryToNonEmptyList(out var list)) {
            return new ReverseIterator<T>(list);
        }
        else {
            return new ReverseIterator<T>(Array.Empty<T>());
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct ReverseIterator<T>(IList<T> list)
    {
        private int _index;

        public ReverseIterator<T> GetEnumerator() => this with { _index = list.Count };

        public readonly T Current => list[_index];

        public bool MoveNext()
        {
            var index = _index - 1;
            if (index < 0)
                return false;

            _index = index;
            return true;
        }
    }
}
