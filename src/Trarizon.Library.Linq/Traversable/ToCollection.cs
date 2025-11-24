using System.Collections;

namespace Trarizon.Library.Linq;

public static partial class TraTraversable
{
    public static IChildrenTraversable<T> AsChildrenTraversable<T>(this IChildrenTraversable<T> source)
        => source;

    public static IEnumerable<T> EnumerateChildren<T>(this IChildrenTraversable<T> source)
        => new ChildrenEnumerable<T>(source);

    private sealed class ChildrenEnumerable<T>(IChildrenTraversable<T> source) : IEnumerable<T>
    {
        public IEnumerator<T> GetEnumerator() => source.GetChildrenEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
