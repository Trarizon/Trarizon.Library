using System.ComponentModel;
using Trarizon.Library.Collections.AllocOpt;

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
}
