using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Collections.Generic;
public partial class PrefixTree<T>
{
    private Entry[] _entries;
    private int _entryCount;
    // _entryCount + free count
    private int _consumedCount;
    // Index of first item of free list, -1 if no free item
    private int _freeFirstIndex;
    private int _version;
    private IEqualityComparer<T>? _comparer;

    public PrefixTree()
    {
        _entries = [];
        _freeFirstIndex = -1;
    }

    public PrefixTree(IEqualityComparer<T> comparer)
    {
        _entries = [];
        _comparer = comparer;
        _freeFirstIndex = -1;
    }

    #region Access

    public bool Contains(ReadOnlySpan<T> sequence)
    {
        var entryIndex = FindPrefix(sequence);
        if (entryIndex == -1)
            return false;
        return _entries[entryIndex].IsEnd;
    }

    public bool Contains(IEnumerable<T> sequence)
    {
        var entryIndex = FindPrefix(sequence);
        if (entryIndex == -1)
            return false;
        return _entries[entryIndex].IsEnd;
    }

    public bool ContainsPrefix(ReadOnlySpan<T> prefix)
    {
        var entryIndex = FindPrefix(prefix);
        if (entryIndex == -1)
            return false;
        return true;
    }

    public bool ContainsPrefix(IEnumerable<T> prefix)
    {
        var entryIndex = FindPrefix(prefix);
        if (entryIndex == -1)
            return false;
        return true;
    }

    private int FindPrefix(ReadOnlySpan<T> prefix)
    {
        if (_entries.Length == 0)
            return -1;

        int entryIndex = 0;

        if (typeof(T).IsValueType && _comparer is null) {
            foreach (var val in prefix) {
                ref readonly var entry = ref _entries[entryIndex];
                int childIndex = entry.Child;
                while (childIndex > 0) {
                    ref var child = ref _entries[childIndex];
                    if (EqualityComparer<T>.Default.Equals(child.Value, val)) {
                        entryIndex = childIndex;
                        goto ContinueFor;
                    }
                    childIndex = child.NextSibling;
                }
                return -1;

            ContinueFor:
                continue;
            }
        }
        else {
            var comparer = _comparer ??= EqualityComparer<T>.Default;
            foreach (var val in prefix) {
                ref readonly var entry = ref _entries[entryIndex];
                int siblingIndex = entry.Child;
                while (siblingIndex > 0) {
                    ref var sibling = ref _entries[siblingIndex];
                    if (comparer.Equals(entry.Value, val)) {
                        entryIndex = siblingIndex;
                        goto ContinueFor;
                    }
                    siblingIndex = sibling.NextSibling;
                }
                return -1;

            ContinueFor:
                continue;
            }
        }
        return entryIndex;
    }

    private int FindPrefix(IEnumerable<T> prefix)
    {
        if (_entries.Length == 0)
            return -1;

        int entryIndex = 0;

        if (typeof(T).IsValueType && _comparer is null) {
            foreach (var val in prefix) {
                ref readonly var entry = ref _entries[entryIndex];
                int childIndex = entry.Child;
                while (childIndex > 0) {
                    ref var child = ref _entries[childIndex];
                    if (EqualityComparer<T>.Default.Equals(child.Value, val)) {
                        entryIndex = childIndex;
                        goto ContinueFor;
                    }
                    childIndex = child.NextSibling;
                }
                return -1;

            ContinueFor:
                continue;
            }
        }
        else {
            var comparer = _comparer ??= EqualityComparer<T>.Default;
            foreach (var val in prefix) {
                ref readonly var entry = ref _entries[entryIndex];
                int siblingIndex = entry.Child;
                while (siblingIndex > 0) {
                    ref var sibling = ref _entries[siblingIndex];
                    if (comparer.Equals(entry.Value, val)) {
                        entryIndex = siblingIndex;
                        goto ContinueFor;
                    }
                    siblingIndex = sibling.NextSibling;
                }
                return -1;

            ContinueFor:
                continue;
            }
        }
        return entryIndex;
    }

    #endregion

    #region Modification

    public bool Add(ReadOnlySpan<T> sequence)
    {
        EnsureRootEntry();
        var parentEntryIndex = 0;

        foreach (var value in sequence) {
            parentEntryIndex = GetOrAddChild(parentEntryIndex, value);
        }

        ref var entry = ref _entries[parentEntryIndex];
        _version++;
        if (entry.IsEnd) {
            return false;
        }
        else {
            entry.SetIsEnd();
            return true;
        }
    }

    public bool Add(IEnumerable<T> sequence)
    {
        EnsureRootEntry();
        var parentEntryIndex = 0;

        foreach (var value in sequence) {
            parentEntryIndex = GetOrAddChild(parentEntryIndex, value);
        }

        ref var entry = ref _entries[parentEntryIndex];
        _version++;
        if (entry.IsEnd) {
            return false;
        }
        else {
            entry.SetIsEnd();
            return true;
        }
    }

    public bool Remove(ReadOnlySpan<T> sequence)
    {
        var entryIndex = FindPrefix(sequence);
        if (entryIndex == -1)
            return false;
        ref var entry = ref _entries[entryIndex];
        if (!entry.IsEnd)
            return false;

        entry.SetNotEnd();
        if (entry.Child > 0) {
            // If entry has child, we just set this as not end
            return true;
        }
        else {
            while (!entry.IsEnd && entryIndex > 0) {
                var parentIndex = entry.ParentOrFreeNext;
                FreeEntry(entryIndex);
                entryIndex = parentIndex;
                entry = ref _entries[parentIndex];
            }
            return true;
        }
    }

    public bool Remove(IEnumerable<T> sequence)
    {
        var entryIndex = FindPrefix(sequence);
        if (entryIndex == -1)
            return false;
        ref var entry = ref _entries[entryIndex];
        if (!entry.IsEnd)
            return false;

        entry.SetNotEnd();
        if (entry.Child > 0) {
            // If entry has child, we just set this as not end
            return true;
        }
        else {
            while (!entry.IsEnd && entryIndex > 0) {
                var parentIndex = entry.ParentOrFreeNext;
                FreeEntry(entryIndex);
                entryIndex = parentIndex;
                entry = ref _entries[parentIndex];
            }
            return true;
        }
    }

    public void Clear()
    {
        ArrayGrowHelper.FreeManaged(_entries, 0, _consumedCount);
        _entryCount = 0;
        _consumedCount = 0;
        _freeFirstIndex = -1;
        _version++;
    }

    private void EnsureRootEntry()
    {
        if (_entries.Length == 0) {
            ArrayGrowHelper.Grow(ref _entries, 1, 0);
            _entryCount = 1;
            _consumedCount = 1;
            ref var entry = ref _entries[0];
            entry.ParentOrFreeNext = -1;
            entry.Version += 2;
        }
    }

    private int GetOrAddChild(int parentIndex, T value)
    {
        Debug.Assert(parentIndex < _consumedCount);

        var childIndex = _entries[parentIndex].Child;

        if (typeof(T).IsValueType && _comparer is null) {
            while (childIndex > 0) {
                ref var child = ref _entries[childIndex];
                // Exists, return the existing value
                if (EqualityComparer<T>.Default.Equals(value, child.Value))
                    return childIndex;
                childIndex = child.NextSibling;
            }
        }
        else {
            var comparer = _comparer ??= EqualityComparer<T>.Default;
            while (childIndex > 0) {
                ref var child = ref _entries[childIndex];
                if (comparer.Equals(value, child.Value))
                    return childIndex;
                childIndex = child.NextSibling;
            }
        }

        {
            childIndex = AddValueInternal(value);
            // AddValueInternal may replace underlying parent, causing invalid data,
            // so we get parent after AddValueInternal
            ref var parent = ref _entries[parentIndex];
            ref var child = ref _entries[childIndex];
            child.ParentOrFreeNext = parentIndex;
            child.NextSibling = parent.Child;
            parent.Child = childIndex;
            return childIndex;
        }
    }

    /// <returns>Index of new entry</returns>
    private int AddValueInternal(T value)
    {
        ref Entry free = ref Unsafe.NullRef<Entry>();
        int index;
        // Get in free list
        if (_freeFirstIndex >= 0) {
            free = ref _entries[_freeFirstIndex];
            index = _freeFirstIndex;
            _freeFirstIndex = free.ParentOrFreeNext;
        }
        // Use unconsumed entry
        else {
            if (_entryCount == _entries.Length) {
                Debug.Assert(_entryCount == _consumedCount);
                ArrayGrowHelper.Grow(ref _entries, _entryCount + 1, _entryCount);
            }
            free = ref _entries[_entryCount];
            index = _entryCount;
            _consumedCount++;
        }

        Debug.Assert(!Unsafe.IsNullRef(ref free));
        free.Value = value;
        free.Version += 2;
        Debug.Assert(!free.IsFreed);
        _entryCount++;
        return index;
    }

    private void FreeEntry(int index)
    {
        Debug.Assert(index > 0);
        ref var entry = ref _entries[index];
#if NETSTANDARD
        entry.Value = default!;
#else
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            entry.Value = default!;
#endif
        _entries[entry.ParentOrFreeNext].Child = entry.NextSibling;
        entry.ParentOrFreeNext = _freeFirstIndex;
        entry.Version += 2;
        Debug.Assert(entry.IsFreed);
        _freeFirstIndex = index;
    }

    #endregion

    [DebuggerDisplay("{Value} ^{ParentOrFreeNext} v{Child} >{NextSibling}")]
    internal struct Entry
    {
        public int ParentOrFreeNext;
        // 0 as invalid, as _entries[0] is always the root
        public int Child;
        // 0 as invalid, as _entries[0] is always the root
        public int NextSibling;
        /// <summary>
        /// Version of the entry, change when toggle free status
        /// use for validation in node, as we cannot ensure the value at some index
        /// won't be changed
        /// </summary>
        /// <remarks>
        /// As the last second bit is the free mask, the field should increment by 2
        /// when update
        /// </remarks>
        public ushort Version;
        public T Value;

        public readonly bool IsFreed => (Version & FreedVersionMask) == 0;
        public readonly bool IsEnd => (Version & IsEndVersionMask) != 0;

        public void SetIsEnd() => Version |= IsEndVersionMask;
        public void SetNotEnd() => Version &= NotEndVersionMask;

        public readonly bool VersionEqualsTo(in Entry other)
        {
            return (Version & NotEndVersionMask) == (other.Version & NotEndVersionMask);
        }

        // Check if the entry is a end
        // is end if the bit is 1
        private const ushort IsEndVersionMask = 1;
        private const ushort NotEndVersionMask = ushort.MaxValue - IsEndVersionMask;
        // Check if the entry is in free list
        // freed if the bit is 0
        private const ushort FreedVersionMask = 1 << 1;
    }
}
