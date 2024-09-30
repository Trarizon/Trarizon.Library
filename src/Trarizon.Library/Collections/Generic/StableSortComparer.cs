using Trarizon.Library.CodeGeneration;

namespace Trarizon.Library.Collections;
public abstract class StableSortComparer<T> : IComparer<(int Index, T Item)>
{
    public static StableSortComparer<T> Default => DefaultStableSortComparer<T>.Default;

    public static StableSortComparer<T> Create(Comparison<T> comparison) => new ComparisonStableSortComparer<T>(comparison);

    public int Compare((int Index, T Item) x, (int Index, T Item) y)
    {
        var res = CompareItem(x.Item, y.Item);
        if (res == 0) {
            res = x.Index.CompareTo(y.Index);
        }
        return res;
    }

    protected abstract int CompareItem(T x, T y);
}

[Singleton(InstancePropertyName = nameof(Comparer<T>.Default), Options = SingletonOptions.NoProvider)]
internal sealed partial class DefaultStableSortComparer<T> : StableSortComparer<T>
{
    protected override int CompareItem(T x, T y) => Comparer<T>.Default.Compare(x, y);
}

internal sealed class ComparisonStableSortComparer<T>(Comparison<T> comparison) : StableSortComparer<T>
{
    protected override int CompareItem(T x, T y) => comparison(x, y);
}