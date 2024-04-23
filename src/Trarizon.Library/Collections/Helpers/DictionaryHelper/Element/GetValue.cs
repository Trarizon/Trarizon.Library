using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Trarizon.Library.Collections.Helpers;
partial class DictionaryHelper
{
    public static ref TValue GetValueRef<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) where TKey : notnull
    {
        ref TValue val = ref CollectionsMarshal.GetValueRefOrNullRef(dictionary, key);
        if (Unsafe.IsNullRef(ref val)) {
            ThrowHelper.ThrowKeyNotFound(key);
        }
        return ref val;
    }
}
