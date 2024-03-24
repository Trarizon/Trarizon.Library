using Trarizon.Library.Collections.Helpers.Utilities.Queriers;

namespace Trarizon.Library.Collections.Helpers;
partial class EnumerableQuery
{
    public static IEnumerable<(T, T2)> CartesianProduct<T, T2>(this IEnumerable<T> first, IEnumerable<T2> second)
    {
        if (first is IList<T> firstList && second is IList<T2> secondList) {
            return firstList.CartesianProductList(secondList);
        }

        if (first.TryGetNonEnumeratedCount(out var count) && count == 0) {
            return Enumerable.Empty<(T, T2)>();
        }
        if (second.TryGetNonEnumeratedCount(out count) && count == 0) {
            return Enumerable.Empty<(T, T2)>();
        }
        return new CartesianProductQuerier<T, T2>(first, second);
    }

    private sealed class CartesianProductQuerier<T, T2>(IEnumerable<T> source, IEnumerable<T2> sub) : SimpleEnumerationQuerier<T, (T, T2)>(source)
    {
        private readonly IEnumerable<T2> _subCollection = sub;
        private IEnumerator<T2>? _subEnumerator;

        public override bool MoveNext()
        {
            const int IterateMain = MinPreservedState - 1;
            const int IterateSub = IterateMain - 1;
            const int End = IterateSub - 1;

            switch (_state) {
                case -1:
                    _enumerator = _source.GetEnumerator();
                    goto case IterateMain;
                case IterateMain: // Not state
                    if (_enumerator!.MoveNext()) {
                        _current.Item1 = _enumerator.Current;
                        _subEnumerator = _subCollection.GetEnumerator();
                        _state = IterateSub;
                        goto case IterateSub;
                    }
                    else {
                        _state = End;
                        return false;
                    }
                case IterateSub:
                    if (_subEnumerator!.MoveNext()) {
                        _current.Item2 = _subEnumerator.Current;
                        return true;
                    }
                    else {
                        _subEnumerator.Dispose();
                        // IterateMain state wont return without change _state
                        // So here is unnecessary to reassign _state
                        goto case IterateMain;
                    }
                case End:
                default:
                    return false;
            }
        }

        protected override EnumerationQuerier<(T, T2)> Clone() => new CartesianProductQuerier<T, T2>(_source, _subCollection);

        protected override void DisposeInternal()
        {
            base.DisposeInternal();
            _subEnumerator?.Dispose();
        }
    }
}
