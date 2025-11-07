namespace Trarizon.Library.Linq;

partial class TraEnumerable
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

    partial class CollectionIteratorBase<T>
    {
        internal sealed override int TryGetCheapCount(out bool exists)
        {
            exists = true;
            return Count;
        }
    }

    partial class ListIteratorBase<T>
    {
        internal sealed override T TryCheapAt(int index, out bool exists)
        {
            exists = true;
            return this[index];
        }

        internal override T TryGetFirst(out bool exists)
        {
            exists = true;
            return this[0];
        }

        internal override T TryGetLast(out bool exists)
        {
            exists = true;
            return this[Count - 1];
        }
    }
}
