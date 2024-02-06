using System.ComponentModel;

namespace Trarizon.Library.Collections.AllocOpt;
[EditorBrowsable(EditorBrowsableState.Never)]
public static class AllocOptCollectionBuilder
{
    public static AllocOptList<T> CreateList<T>(ReadOnlySpan<T> values)
    {
        var list = new AllocOptList<T>(values.Length);
        list.AddRange(values);
        return list;
    }

    public static AllocOptStack<T> CreateStack<T>(ReadOnlySpan<T> values)
    {
        var stack = new AllocOptStack<T>(values.Length);
        stack.PushRange(values);
        return stack;
    }
}
