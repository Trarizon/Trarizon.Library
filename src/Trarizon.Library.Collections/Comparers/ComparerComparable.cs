#if NETSTANDARD
#pragma warning disable CS8604 // 引用类型参数可能为 null。
#endif

using System.Runtime.CompilerServices;

namespace Trarizon.Library.Collections.Comparers;

public readonly struct ComparerComparable<T,TComparer>(T value,TComparer comparer) : IComparable<T> where TComparer : IComparer<T>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(T? other) => comparer.Compare(value, other);
}
