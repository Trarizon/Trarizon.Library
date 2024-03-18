#if !NET9_0_OR_GREATER

using Trarizon.Library.Collections.Helpers.Utilities.Queriers;

namespace Trarizon.Library.Collections.Helpers;
partial class EnumerableQuery
{
    public static IEnumerable<(int Index, T Item)> Index<T>(this IEnumerable<T> source)
    {
        if (source.IsCheapEmpty())
            return Enumerable.Empty<(int, T)>();

        return new IndexQuery<T>(source);
    }

    private sealed class IndexQuery<T>(IEnumerable<T> source) : SimpleEnumerationQuerier<T, (int, T)>(source)
    {
        private int _index = 0;

        public override bool MoveNext()
        {
            const int Iterate = MinPreservedState - 1;
            const int End = Iterate - 1;

            switch (_state) {
                case -1:
                    InitializeEnumerator();
                    _state = Iterate;
                    goto case Iterate;
                case Iterate:
                    if (_enumerator.MoveNext()) {
                        _current = (_index, _enumerator.Current);
                        _index++;
                        return true;
                    }
                    else {
                        _state = End;
                        return false;
                    }
                default:
                    return false;
            }
        }

        protected override EnumerationQuerier<(int, T)> Clone() => new IndexQuery<T>(_source);
    }
}

#endif
