namespace Trarizon.Library.Collections.Extensions.Helpers;
internal sealed class StableSortComparer<T>(Comparison<T> comparison) : IComparer<(int, T)>
{
    public static StableSortComparer<T> Default = new(Comparer<T>.Default.Compare);

    public int Compare((int, T) x, (int, T) y)
    {
        var result = comparison(x.Item2, y.Item2);
        return result != 0 ? result : x.Item1.CompareTo(y.Item1);
    }
}
