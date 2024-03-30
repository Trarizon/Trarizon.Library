using BenchmarkDotNet.Attributes;
using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Test.Run.HistoryBenchmarks;
// 对比了使用Func<>包装和使用接口泛型包装的性能
// 结果而言用接口包装Func<>和直接使用Func<>差不多
// 使用Func<>包装lambda比使用接口慢，而且需要分配Func<>的空间
[MemoryDiagnoser]
public class Func_Vs_InterfaceGenericDispatch
{
    public IEnumerable<object[]> ArgsSource()
    {
        yield return new IEnumerable<int>[] {
            [1,3,5,7,9,],
            [1,5,6,8,9,],
        };
        yield return new IEnumerable<int>[] {
            [1,3,5,7,9,10,10,25,68],
            [1,5,6,8,9,10,24,25,68],
        };
    }

    [Benchmark]
    [ArgumentsSource(nameof(ArgsSource))]
    public void Merge(IEnumerable<int> first, IEnumerable<int> second)
    {
        foreach (var item in first.Merge(second))
        {
            _ = item;
        }
    }

    //[Benchmark]
    //[ArgumentsSource(nameof(ArgsSource))]
    //public void MergeNew(IEnumerable<int> first, IEnumerable<int> second)
    //{
    //    foreach (var item in first.MergeNew(second))
    //    {
    //        _ = item;
    //    }
    //}

    [Benchmark]
    [ArgumentsSource(nameof(ArgsSource))]
    public void MergeFunc(IEnumerable<int> first, IEnumerable<int> second)
    {
        foreach (var item in first.Merge(second, (l, r) => l <= r)) {
            _ = item;
        }
    }

    //[Benchmark]
    //[ArgumentsSource(nameof(ArgsSource))]
    //public void MergeFuncNew(IEnumerable<int> first, IEnumerable<int> second)
    //{
    //    foreach (var item in first.MergeNew(second, (l, r) => l <= r)) {
    //        _ = item;
    //    }
    //}
}


// in EnumerableQuery


//public static IEnumerable<T> MergeNew<T>(this IEnumerable<T> first, IEnumerable<T> second, bool descending = false) where T : IComparisonOperators<T, T, bool>
//{
//    return descending
//        ? new MergeNewQuerier<T, DescOrderChecker<T>>(first, second, default)
//        : new MergeNewQuerier<T, AscOrderChecker<T>>(first, second, default);
//}

//public static IEnumerable<T> MergeNew<T>(this IEnumerable<T> first, IEnumerable<T> second, Func<T, T, bool> order)
//    => new MergeNewQuerier<T, FuncOrderChecker<T>>(first, second, new(order));

//interface IOrderChecker<T>
//{
//    bool IsInOrder(T left, T right);
//}

//struct FuncOrderChecker<T>(Func<T, T, bool> func) : IOrderChecker<T>
//{
//    public readonly bool IsInOrder(T left, T right) => func(left, right);
//}

//struct AscOrderChecker<T> : IOrderChecker<T> where T : IComparisonOperators<T, T, bool>
//{
//    public readonly bool IsInOrder(T left, T right) => left <= right;
//}

//struct DescOrderChecker<T> : IOrderChecker<T> where T : IComparisonOperators<T, T, bool>
//{
//    public readonly bool IsInOrder(T left, T right) => left >= right;
//}

//private sealed class MergeNewQuerier<T, TOrderChecker>(
//    IEnumerable<T> first, IEnumerable<T> second,
//    TOrderChecker orderChecker)
//    : EnumerationQuerier<T>
//    where TOrderChecker : IOrderChecker<T>
//{
//    private readonly IEnumerable<T> _first = first;
//    private readonly IEnumerable<T> _second = second;

//    private IEnumerator<T>? _firstEnumerator;
//    private IEnumerator<T>? _secondEnumerator;
//    private T _current = default!;

//    public override T Current => _current;

//    public override bool MoveNext()
//    {
//        const int IterateFirst = MinPreservedState - 1;
//        const int IterateSecond = IterateFirst - 1;
//        const int IterateFirstOnly = IterateSecond - 1;
//        const int IterateSecondOnly = IterateFirstOnly - 1;
//        const int NoMoreElement = IterateSecondOnly - 1;

//        switch (_state) {
//            case -1:
//                _firstEnumerator = _first.GetEnumerator();
//                _secondEnumerator = _second.GetEnumerator();
//                switch (_firstEnumerator!.MoveNext(), _secondEnumerator.MoveNext()) {
//                    case (true, true): goto CompareAndSetNextState;
//                    case (true, false): goto DisposeSecondEnumerator;
//                    case (false, true): goto DisposeFirstEnumerator;
//                    case (false, false):
//                        _state = NoMoreElement;
//                        return false;
//                }
//            case IterateFirst:
//                if (_firstEnumerator!.MoveNext())
//                    goto CompareAndSetNextState;
//                else
//                    goto DisposeFirstEnumerator;

//            case IterateSecond:
//                if (_secondEnumerator!.MoveNext())
//                    goto CompareAndSetNextState;
//                else
//                    goto DisposeSecondEnumerator;

//            case IterateFirstOnly:
//                if (_firstEnumerator!.MoveNext()) {
//                    _current = _firstEnumerator.Current;
//                    return true;
//                }
//                else {
//                    _state = NoMoreElement;
//                    return false;
//                }

//            case IterateSecondOnly:
//                if (_secondEnumerator!.MoveNext()) {
//                    _current = _secondEnumerator.Current;
//                    return true;
//                }
//                else {
//                    _state = NoMoreElement;
//                    return false;
//                }

//            case NoMoreElement:
//            default:
//                return false;
//        }

//    DisposeFirstEnumerator:
//        _state = IterateSecondOnly;
//        _current = _secondEnumerator!.Current;
//        return true;

//    DisposeSecondEnumerator:
//        _state = IterateFirstOnly;
//        _current = _firstEnumerator!.Current;
//        return true;

//    CompareAndSetNextState:
//        T left = _firstEnumerator!.Current;
//        T right = _secondEnumerator!.Current;

//        if (orderChecker.IsInOrder(left, right)) {
//            _state = IterateFirst;
//            _current = left;
//        }
//        else {
//            _state = IterateSecond;
//            _current = right;
//        }
//        return true;
//    }

//    protected override EnumerationQuerier<T> Clone() => new MergeNewQuerier<T, TOrderChecker>(_first, _second, orderChecker);

//    protected override void DisposeInternal()
//    {
//        _firstEnumerator?.Dispose();
//        _secondEnumerator?.Dispose();
//    }
//}
