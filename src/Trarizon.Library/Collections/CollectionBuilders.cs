using Trarizon.Library.Collections.AllocOpt;

namespace Trarizon.Library.Collections;
public static class CollectionBuilders
{
    public static AllocOptList<T> CreateAllocOptList<T>(ReadOnlySpan<T> values)
    {
        var list = new AllocOptList<T>();
        list.AddRange(values);
        return list;
    }
}
