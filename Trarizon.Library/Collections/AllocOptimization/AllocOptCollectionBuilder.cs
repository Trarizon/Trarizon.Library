using System.ComponentModel;

namespace Trarizon.Library.Collections.AllocOptimization;
[EditorBrowsable(EditorBrowsableState.Never)]
public static class AllocOptCollectionBuilder
{
	public static AllocOptList<T> CreateList<T>(ReadOnlySpan<T> values)
	{
		var list = new AllocOptList<T>(values.Length);
		list.AddRange(values);
		return list;
	}
}
