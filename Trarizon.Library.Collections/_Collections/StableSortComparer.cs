namespace Trarizon.Library.Collections;
public sealed class StableSortComparer<T>(Comparison<T> comparison) : IComparer<(int, T)>
{
    public static readonly StableSortComparer<T> Default = new(Comparer<T>.Default.Compare);

    public StableSortComparer(IComparer<T> comparer) : this(comparer.Compare) { }

    public int Compare((int, T) x, (int, T) y)
    {
        var result = comparison(x.Item2, y.Item2);
        return result != 0 ? result : x.Item1.CompareTo(y.Item1);
    }
}
