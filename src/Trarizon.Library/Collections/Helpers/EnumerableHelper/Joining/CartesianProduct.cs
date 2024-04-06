using System.Diagnostics;
using Trarizon.Library.Collections.Helpers.Queriers;

namespace Trarizon.Library.Collections.Helpers;
partial class EnumerableHelper
{
    public static IEnumerable<(T, T2)> CartesianProduct<T, T2>(this IEnumerable<T> first, IEnumerable<T2> second)
    {
        if(first is IList<T> l1) {
            if (second is IList<T2> l2)
                return l1.CartesianProductList(l2);

            if (first.IsFixedSizeEmpty())
                return [];
        }

        if (second.IsFixedSizeEmpty())
            return [];

        return new CartesianProductQuerier<T, T2>(first, second);
    }


    private sealed class CartesianProductQuerier<T, T2>(IEnumerable<T> source, IEnumerable<T2> sub) : SimpleEnumerationQuerier<T, (T, T2)>(source)
    {
        private readonly IEnumerable<T2> _subCollection = sub;
        private IEnumerator<T2>? _subEnumerator;

        public override bool MoveNext()
        {
            const int IterateMain_PureState = MinPreservedState - 1;
            const int IterateSub = IterateMain_PureState - 1;
            const int End = IterateSub - 1;

            Debug.Assert(_state != IterateMain_PureState);

            switch (_state) {
                case -1:
                    _enumerator = _source.GetEnumerator();
                    goto case IterateMain_PureState;
                case IterateMain_PureState:
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
                        goto case IterateMain_PureState;
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
