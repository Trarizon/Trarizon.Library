namespace Trarizon.Library.Collections;
public static partial class TraEnumerable
{
    partial class IteratorBase<T>
    {
        internal virtual int TryGetCheapCount(out bool exists)
        {
            exists = false;
            return default;
        }

        internal virtual T TryCheapAt(int index, out bool exists)
        {
            exists = false;
            return default!;
        }

        internal virtual T TryGetFirst(out bool exists)
        {
            exists = false;
            return default!;
        }

        internal virtual T TryGetLast(out bool exists)
        {
            exists = false;
            return default!;
        }
    }
}
