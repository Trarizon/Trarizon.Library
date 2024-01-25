using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Trarizon.Library.Collections.Extensions.Helpers;
using Trarizon.Library.Collections.Extensions.Helpers.Queriers;

namespace Trarizon.Library.Collections.Extensions;
partial class EnumerableQuery
{
    /// <summary>
    /// Merge 2 ordered <see cref="IEnumerable{T}"/> into one
    /// </summary>
    /// <param name="order">Indicates the order, default is <c>l &lt; r</c></param>
    public static IEnumerable<T> Merge<T>(this IEnumerable<T> first, IEnumerable<T> second, Func<T, T, bool> order) => new MergeByFuncQuerier<T>(first, second, order);

    /// <summary>
    /// Merge 2 ordered <see cref="IEnumerable{T}"/> into one
    /// </summary>
    /// <param name="order">Indicates the order, default is <c>l &lt; r</c></param>
    public static IEnumerable<T> Merge<T>(this IEnumerable<T> first, IEnumerable<T> second, IComparer<T>? comparer = default, bool descending = false)
        => new MergeByComparerQuerier<T>(first, second, comparer ?? Comparer<T>.Default, descending);

    /// <summary>
    /// Merge 2 ordered <see cref="IEnumerable{T}"/> into one
    /// </summary>
    /// <param name="order">Indicates the order, default is <c>l &lt; r</c></param>
    public static IEnumerable<T> Merge<T>(this IEnumerable<T> first, IEnumerable<T> second, bool descending) where T : IComparisonOperators<T, T, bool>
        => new MergeByFuncQuerier<T>(first, second, descending ? _IsInOrderDesc<T> : _IsInOrderAsc<T>);

    [SuppressMessage("Style", "IDE1006")]
    private static bool _IsInOrderAsc<T>(T left, T right) where T : IComparisonOperators<T, T, bool> => QueryUtil.IsInOrder(left, right, true);
    [SuppressMessage("Style", "IDE1006")]
    private static bool _IsInOrderDesc<T>(T left, T right) where T : IComparisonOperators<T, T, bool> => QueryUtil.IsInOrder(left, right, false);


    private abstract class MergeQuerier<T>(IEnumerable<T> first, IEnumerable<T> second) : EnumerationQuerier<T>
    {
        protected readonly IEnumerable<T> _first = first;
        protected readonly IEnumerable<T> _second = second;

        private IEnumerator<T>? _firstEnumerator;
        private IEnumerator<T>? _secondEnumerator;
        private T _current = default!;

        public override T Current => _current;

        public override bool MoveNext()
        {
            const int IterateFirst = MinPreservedState - 1;
            const int IterateSecond = IterateFirst - 1;
            const int IterateFirstOnly = IterateSecond - 1;
            const int IterateSecondOnly = IterateFirstOnly - 1;
            const int NoMoreElement = IterateSecondOnly - 1;

            switch (_state) {
                case -1:
                    _firstEnumerator = _first.GetEnumerator();
                    _secondEnumerator = _second.GetEnumerator();
                    switch (_firstEnumerator!.MoveNext(), _secondEnumerator.MoveNext()) {
                        case (true, true): goto CompareAndSetNextState;
                        case (true, false): goto DisposeSecondEnumerator;
                        case (false, true): goto DisposeFirstEnumerator;
                        case (false, false):
                            _state = NoMoreElement;
                            return false;
                    }
                case IterateFirst:
                    if (_firstEnumerator!.MoveNext())
                        goto CompareAndSetNextState;
                    else
                        goto DisposeFirstEnumerator;

                case IterateSecond:
                    if (_secondEnumerator!.MoveNext())
                        goto CompareAndSetNextState;
                    else
                        goto DisposeSecondEnumerator;

                case IterateFirstOnly:
                    if (_firstEnumerator!.MoveNext()) {
                        _current = _firstEnumerator.Current;
                        return true;
                    }
                    else {
                        _state = NoMoreElement;
                        return false;
                    }

                case IterateSecondOnly:
                    if (_secondEnumerator!.MoveNext()) {
                        _current = _secondEnumerator.Current;
                        return true;
                    }
                    else {
                        _state = NoMoreElement;
                        return false;
                    }

                case NoMoreElement:
                default:
                    return false;
            }

        DisposeFirstEnumerator:
            _state = IterateSecondOnly;
            _current = _secondEnumerator!.Current;
            return true;

        DisposeSecondEnumerator:
            _state = IterateFirstOnly;
            _current = _firstEnumerator!.Current;
            return true;

        CompareAndSetNextState:
            T left = _firstEnumerator!.Current;
            T right = _secondEnumerator!.Current;

            if (InOrder(left, right)) {
                _state = IterateFirst;
                _current = left;
            }
            else {
                _state = IterateSecond;
                _current = right;
            }
            return true;
        }

        protected abstract bool InOrder(T left, T right);

        protected override void DisposeInternal()
        {
            _firstEnumerator?.Dispose();
            _secondEnumerator?.Dispose();
        }
    }

    private sealed class MergeByFuncQuerier<T>(
        IEnumerable<T> first, IEnumerable<T> second,
        Func<T, T, bool> order)
        : MergeQuerier<T>(first, second)
    {
        protected override EnumerationQuerier<T> Clone() => new MergeByFuncQuerier<T>(_first, _second, order);
        protected override bool InOrder(T left, T right) => order(left, right);
    }

    private sealed class MergeByComparerQuerier<T>(
        IEnumerable<T> first, IEnumerable<T> second,
        IComparer<T> comparer,
        bool descending)
        : MergeQuerier<T>(first, second)
    {
        protected override EnumerationQuerier<T> Clone() => new MergeByComparerQuerier<T>(_first, _second, comparer, descending);
        protected override bool InOrder(T left, T right) => QueryUtil.IsInOrder(left, right, comparer, descending);
    }
}
