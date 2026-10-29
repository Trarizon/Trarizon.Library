using System.Numerics;

namespace Trarizon.Library.Linq;

internal interface IOrderComparer<T>
{
    bool Less(T x, T y);
    bool LessOrEqual(T x, T y);
    bool Greater(T x, T y);
    bool GreaterOrEqual(T x, T y);
}

internal readonly struct CmpOrder<T>(IComparer<T> comparer) : IOrderComparer<T>
{
    public bool Less(T x, T y) => comparer.Compare(x, y) < 0;
    public bool LessOrEqual(T x, T y) => comparer.Compare(x, y) <= 0;
    public bool Greater(T x, T y) => comparer.Compare(x, y) > 0;
    public bool GreaterOrEqual(T x, T y) => comparer.Compare(x, y) >= 0;
}

internal readonly struct RCmpOrder<T>(IComparer<T> comparer) : IOrderComparer<T>
{
    public bool Less(T x, T y) => comparer.Compare(x, y) > 0;
    public bool LessOrEqual(T x, T y) => comparer.Compare(x, y) >= 0;
    public bool Greater(T x, T y) => comparer.Compare(x, y) < 0;
    public bool GreaterOrEqual(T x, T y) => comparer.Compare(x, y) <= 0;
}

internal readonly struct FuncOrder<T>(Func<T, T, int> compare) : IOrderComparer<T>
{
    public bool Less(T x, T y) => compare(x, y) < 0;
    public bool LessOrEqual(T x, T y) => compare(x, y) <= 0;
    public bool Greater(T x, T y) => compare(x, y) > 0;
    public bool GreaterOrEqual(T x, T y) => compare(x, y) >= 0;
}

#if NET7_0_OR_GREATER

internal readonly struct OpAscOrder<T> : IOrderComparer<T> where T : IComparisonOperators<T, T, bool>
{
    public bool Less(T x, T y) => x < y;
    public bool LessOrEqual(T x, T y) => x <= y;
    public bool Greater(T x, T y) => x > y;
    public bool GreaterOrEqual(T x, T y) => x >= y;
}

internal readonly struct OpDescOrder<T> : IOrderComparer<T> where T : IComparisonOperators<T, T, bool>
{
    public bool Less(T x, T y) => x > y;
    public bool LessOrEqual(T x, T y) => x >= y;
    public bool Greater(T x, T y) => x < y;
    public bool GreaterOrEqual(T x, T y) => x <= y;
}

#endif