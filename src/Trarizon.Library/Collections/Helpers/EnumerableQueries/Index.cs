#if !NET9_0_OR_GREATER

using Trarizon.Library.Collections.Helpers.Queriers;

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
        public override bool MoveNext()
        {
            const int End = MinPreservedState - 1;

            switch (_state) {
                case -1:
                    InitializeEnumerator();
                    goto Iterating;
                case >= 0:
                Iterating:
                    if (_enumerator.MoveNext()) {
                        _state++;
                        _current = (_state, _enumerator.Current);
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
