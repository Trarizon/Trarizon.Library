using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Trarizon.Library.Collections.Helpers;
#if NETSTANDARD2_0
using RuntimeHelpers = Trarizon.Library.Collections.Helpers.PfRuntimeHelpers;
#endif

namespace Trarizon.Library.Collections.Generic;
[Experimental("TRALIB")]
public class ContiguousPrefixTree<T>
{
    private Entry[] _entries;
    private int _count;
    private int _entryCount;
    // _entryCount + free count
    private int _consumedCount;
    // Index of first item of free list, -1 if no free item
    private int _freeFirstIndex;
    private int _version;
    private IEqualityComparer<T>? _comparer;

    public ContiguousPrefixTree()
    {
        _entries = [];
        _freeFirstIndex = -1;
    }

    public ContiguousPrefixTree(IEqualityComparer<T> comparer)
    {
        _entries = [];
        _comparer = comparer;
        _freeFirstIndex = -1;
    }

    #region Access

    public int Count => _count;

    public Node RootNode
    {
        get {
            if (_entries.Length == 0)
                return default;
            return new Node(this, 0);
        }
    }

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

    public Node GetOrAdd(ReadOnlySpan<T> sequence)
    {
        EnsureRootEntry();
        var entryIndex = 0;

        foreach (var value in sequence) {
            entryIndex = GetOrAddChild(entryIndex, value);
        }

        ref var entry = ref _entries[entryIndex];
        _version++;
        _count++;
        entry.SetIsEnd();
        return new Node(this, entryIndex);
    }

    public Node GetOrAdd(IEnumerable<T> sequence)
    {
        EnsureRootEntry();
        var entryIndex = 0;

        foreach (var value in sequence) {
            entryIndex = GetOrAddChild(entryIndex, value);
        }

        ref var entry = ref _entries[entryIndex];
        _version++;
        _count++;
        entry.SetIsEnd();
        return new Node(this, entryIndex);
    }

    /// <summary>
    /// Try add a sequence
    /// </summary>
    public bool TryAdd(ReadOnlySpan<T> sequence, out Node node)
    {
        EnsureRootEntry();
        var entryIndex = 0;

        foreach (var value in sequence) {
            entryIndex = GetOrAddChild(entryIndex, value);
        }

        ref var entry = ref _entries[entryIndex];
        _version++;
        _count++;
        if (entry.IsEnd) {
            node = default;
            return false;
        }
        else {
            entry.SetIsEnd();
            node = new Node(this, entryIndex);
            return true;
        }
    }

    /// <summary>
    /// Try add a sequence
    /// </summary>
    public bool TryAdd(IEnumerable<T> sequence, out Node node)
    {
        EnsureRootEntry();
        var entryIndex = 0;

        foreach (var value in sequence) {
            entryIndex = GetOrAddChild(entryIndex, value);
        }

        ref var entry = ref _entries[entryIndex];
        _version++;
        _count++;
        if (entry.IsEnd) {
            node = default;
            return false;
        }
        else {
            entry.SetIsEnd();
            node = new Node(this, entryIndex);
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

        _version++;
        _count--;
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

        _version++;
        _count--;
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
        ArrayGrowHelper.FreeIfReferenceOrContainsReferences(_entries.AsSpan(0, _consumedCount));
        _count = 0;
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
            childIndex = GetFreeEntry();
            // AddValueInternal may replace underlying parent, causing invalid data,
            // so we get parent after AddValueInternal
            ref var parent = ref _entries[parentIndex];
            ref var child = ref _entries[childIndex];
            child = new()
            {
                Value = value,
                ParentOrFreeNext = parentIndex,
                NextSibling = parent.Child,
                Version = child.Version,
            };
            parent.Child = childIndex;
            return childIndex;
        }
    }

    /// <returns>Index of new entry</returns>
    private int GetFreeEntry()
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
        free.Version += 2;
        Debug.Assert(!free.IsFreed);
        _entryCount++;
        return index;
    }

    private void FreeEntry(int index)
    {
        Debug.Assert(index > 0);
        ref var entry = ref _entries[index];
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            entry.Value = default!;
     
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
        public int Version;
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
        private const int IsEndVersionMask = 1;
        private const int NotEndVersionMask = int.MaxValue - IsEndVersionMask;
        // Check if the entry is in free list
        // freed if the bit is 0
        private const int FreedVersionMask = 1 << 1;
    }

    public readonly struct Node : IEquatable<Node>
    {
        internal readonly ContiguousPrefixTree<T> _tree;
        internal readonly int _index;
        private readonly int _version;

        internal Node(ContiguousPrefixTree<T> tree, int index)
        {
            Debug.Assert(!tree._entries[index].IsFreed);
            _tree = tree;
            _index = index;
            _version = tree._entries[index].Version;
        }

        public bool HasValue => _tree is not null && _version == _tree._entries[_index].Version;

        public bool IsEnd => GetValidatedEntryRef().IsEnd;

        public T Value => GetValidatedEntryRef().Value;

        public ref readonly T ValueRef => ref GetValidatedEntryRef().Value;

        public Node Parent
        {
            get {
                ref var entry = ref GetValidEntryRefOrNullRef();
                if (Unsafe.IsNullRef(ref entry))
                    return default;
                if (_index == 0)
                    return default;
                return new Node(_tree, entry.ParentOrFreeNext);
            }
        }

        public ChildNodesCollection Children => new(this);

        internal ref Entry GetValidEntryRefOrNullRef()
        {
            if (_tree is null)
                goto Invalid;
            ref var entry = ref _tree._entries[_index];
            if (entry.Version != _version)
                goto Invalid;
            Debug.Assert(!entry.IsFreed);
            return ref entry;

        Invalid:
            return ref Unsafe.NullRef<Entry>();
        }

        private ref Entry GetValidatedEntryRef()
        {
            ref var entry = ref _tree._entries[_index];
            if (_version != entry.Version)
                Throws.NodeIsInvalidated();
            Debug.Assert(!entry.IsFreed);
            return ref entry;
        }

        #region Equality

        public bool Equals(Node other) => _tree == other._tree && _index == other._index;
        public override bool Equals([NotNullWhen(true)] object? obj) => obj is Node node && Equals(node);
        public override int GetHashCode() => HashCode.Combine(_tree, _index);
        public static bool operator ==(Node left, Node right) => left.Equals(right);
        public static bool operator !=(Node left, Node right) => !(left == right);

        #endregion
    }

    public readonly struct ChildNodesCollection : IEnumerable<Node>
    {
        private readonly Node _node;

        public bool IsEmpty
        {
            get {
                ref var entry = ref _node.GetValidEntryRefOrNullRef();
                if (Unsafe.IsNullRef(ref entry))
                    return false;
                return entry.Child == 0;
            }
        }

        internal ChildNodesCollection(Node node)
        {
            _node = node;
        }

        public readonly Enumerator GetEnumerator()
        {
            ref var entry = ref _node.GetValidEntryRefOrNullRef();
            if (Unsafe.IsNullRef(ref entry))
                return Enumerator.Empty;
            var firstChild = entry.Child;
            if (firstChild > 0)
                return new(_node._tree, firstChild);
            return Enumerator.Empty;
        }

        readonly IEnumerator<Node> IEnumerable<Node>.GetEnumerator() => GetEnumerator();
        readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<Node>
        {
            private readonly ContiguousPrefixTree<T> _tree;
            private readonly int _version;
            private int _index;
            private int _current;

            internal static Enumerator Empty => new(-1);

            internal Enumerator(ContiguousPrefixTree<T> tree, int firstSiblingIndex)
            {
                _tree = tree;
                _version = tree._version;
                _index = firstSiblingIndex;
            }

            private Enumerator(int index)
            {
                Debug.Assert(index < 0);
                _index = -1;
                _tree = null!;
            }

            public readonly Node Current => new Node(_tree, _current);

            public bool MoveNext()
            {
                if (_index < 0)
                    return false;

                ValidateVersion();

                _current = _index;
                _index = _tree._entries[_index].NextSibling;
                if (_index <= 0)
                    _index = -1;
                return true;
            }

            private readonly void ValidateVersion()
            {
                if (_version != _tree._version)
                    Throws.CollectionModifiedAfterEnumeratorCreated();
            }

            void IEnumerator.Reset() => throw new NotImplementedException();
            readonly object? IEnumerator.Current => Current;
            readonly void IDisposable.Dispose() { }
        }
    }
}
