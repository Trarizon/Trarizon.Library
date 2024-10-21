using System.ComponentModel;

namespace Trarizon.Library.Collections;
partial class TraIter
{
    /// <summary>
    /// Reverse, because official Linq will always create a cache, so this will be
    /// better when only iterate once,,, maybe
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
