using CommunityToolkit.Diagnostics;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Library.Collections.Generic;
public class PrefixTreeDictionary<TKey, TValue> where TKey : notnull
{
    private Entry[] _entries;
    private int _count;
    private int _entryCount;
    // _entryCount + free count
    private int _consumedCount;
    // Index of first item of free list, -1 if no free item
    private int _freeFirstIndex;
    private int _version;
    private IEqualityComparer<TKey>? _comparer;

    public PrefixTreeDictionary()
    {
        _entries = [];
        _freeFirstIndex = -1;
    }

    public PrefixTreeDictionary(IEqualityComparer<TKey> comparer)
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

    public bool TryGetValue(ReadOnlySpan<TKey> sequence, [MaybeNullWhen(false)] out TValue value)
    {
        var entryIndex = FindPrefix(sequence);
        if (entryIndex == -1) {
            value = default;
            return false;
        }
        ref readonly var entry = ref _entries[entryIndex];
        if (entry.IsEnd) {
            value = entry.Value;
            return true;
        }
        else {
            value = default;
            return false;
        }
    }

    public bool TryGetValue(IEnumerable<TKey> sequence, [MaybeNullWhen(false)] out TValue value)
    {
        var entryIndex = FindPrefix(sequence);
        if (entryIndex == -1) {
            value = default;
            return false;
        }
        ref readonly var entry = ref _entries[entryIndex];
        if (entry.IsEnd) {
            value = entry.Value;
            return true;
        }
        else {
            value = default;
            return false;
        }
    }

    public bool ContainsKey(ReadOnlySpan<TKey> sequence)
    {
        var entryIndex = FindPrefix(sequence);
        if (entryIndex == -1)
            return false;
        return _entries[entryIndex].IsEnd;
    }

    public bool ContainsKey(IEnumerable<TKey> sequence)
    {
        var entryIndex = FindPrefix(sequence);
        if (entryIndex == -1)
            return false;
        return _entries[entryIndex].IsEnd;
    }

    public bool ContainsKeyPrefix(ReadOnlySpan<TKey> prefix)
    {
        var entryIndex = FindPrefix(prefix);
        if (entryIndex == -1)
            return false;
        return true;
    }

    public bool ContainsKeyPrefix(IEnumerable<TKey> prefix)
    {
        var entryIndex = FindPrefix(prefix);
        if (entryIndex == -1)
            return false;
        return true;
    }

    private int FindPrefix(ReadOnlySpan<TKey> prefix)
    {
        if (_entries.Length == 0)
            return -1;

        int entryIndex = 0;

        if (typeof(TKey).IsValueType && _comparer is null) {
            foreach (var val in prefix) {
                ref readonly var entry = ref _entries[entryIndex];
                int childIndex = entry.Child;
                while (childIndex > 0) {
                    ref var child = ref _entries[childIndex];
                    if (EqualityComparer<TKey>.Default.Equals(child.Key, val)) {
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
            var comparer = _comparer ??= EqualityComparer<TKey>.Default;
            foreach (var val in prefix) {
                ref readonly var entry = ref _entries[entryIndex];
                int siblingIndex = entry.Child;
                while (siblingIndex > 0) {
                    ref var sibling = ref _entries[siblingIndex];
                    if (comparer.Equals(entry.Key, val)) {
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

    private int FindPrefix(IEnumerable<TKey> prefix)
    {
        if (_entries.Length == 0)
            return -1;

        int entryIndex = 0;

        if (typeof(TKey).IsValueType && _comparer is null) {
            foreach (var val in prefix) {
                ref readonly var entry = ref _entries[entryIndex];
                int childIndex = entry.Child;
                while (childIndex > 0) {
                    ref var child = ref _entries[childIndex];
                    if (EqualityComparer<TKey>.Default.Equals(child.Key, val)) {
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
            var comparer = _comparer ??= EqualityComparer<TKey>.Default;
            foreach (var val in prefix) {
                ref readonly var entry = ref _entries[entryIndex];
                int siblingIndex = entry.Child;
                while (siblingIndex > 0) {
                    ref var sibling = ref _entries[siblingIndex];
                    if (comparer.Equals(entry.Key, val)) {
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

    public Node GetOrAdd(ReadOnlySpan<TKey> sequence, TValue value)
    {
        EnsureRootEntry();
        var entryIndex = 0;

        foreach (var key in sequence) {
            entryIndex = GetOrAddChild(entryIndex, key, value);
        }

        ref var entry = ref _entries[entryIndex];
        _version++;
        _count++;
        entry.SetIsEnd();
        return new Node(this, entryIndex);
    }

    public Node GetOrAdd(IEnumerable<TKey> sequence, TValue value)
    {
        EnsureRootEntry();
        var entryIndex = 0;

        foreach (var key in sequence) {
            entryIndex = GetOrAddChild(entryIndex, key, value);
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
    public bool TryAdd(ReadOnlySpan<TKey> sequence, TValue value, out Node node)
    {
        EnsureRootEntry();
        var entryIndex = 0;

        foreach (var key in sequence) {
            entryIndex = GetOrAddChild(entryIndex, key, value);
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
    public bool TryAdd(IEnumerable<TKey> sequence, TValue value, out Node node)
    {
        EnsureRootEntry();
        var entryIndex = 0;

        foreach (var key in sequence) {
            entryIndex = GetOrAddChild(entryIndex, key, value);
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

    public bool Remove(ReadOnlySpan<TKey> sequence)
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

    public bool Remove(IEnumerable<TKey> sequence)
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

    private int GetOrAddChild(int parentIndex, TKey key, TValue value)
    {
        Debug.Assert(parentIndex < _consumedCount);

        var childIndex = _entries[parentIndex].Child;

        if (typeof(TKey).IsValueType && _comparer is null) {
            while (childIndex > 0) {
                ref var child = ref _entries[childIndex];
                // Exists, return the existing value
                if (EqualityComparer<TKey>.Default.Equals(key, child.Key))
                    return childIndex;
                childIndex = child.NextSibling;
            }
        }
        else {
            var comparer = _comparer ??= EqualityComparer<TKey>.Default;
            while (childIndex > 0) {
                ref var child = ref _entries[childIndex];
                if (comparer.Equals(key, child.Key))
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
                Key = key,
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

#if NETSTANDARD
        entry.Key = default!;
        entry.Value = default!;
#else
        if (RuntimeHelpers.IsReferenceOrContainsReferences<TKey>())
            entry.Key = default!;
        if (RuntimeHelpers.IsReferenceOrContainsReferences<TValue>())
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
        public TKey Key;
        public TValue Value;

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

    public readonly struct Node : IEquatable<Node>
    {
        internal readonly PrefixTreeDictionary<TKey, TValue> _tree;
        internal readonly int _index;

        internal Node(PrefixTreeDictionary<TKey, TValue> tree, int index)
        {
            _tree = tree;
            _index = index;
        }

        public bool HasKey => _tree is not null;

        public bool IsEnd => EntryRef.IsEnd;

        public TKey Key => KeyRef;

        public ref readonly TKey KeyRef => ref EntryRef.Key;

        public TValue Value
        {
            get {
                if (!IsEnd)
                    ThrowHelper.ThrowInvalidOperationException("The node doesn't contains a value");
                return EntryRef.Value;
            }
            set {
                if (!IsEnd)
                    ThrowHelper.ThrowInvalidOperationException("The node doesn't contains a value");
                EntryRef.Value = value;
                _tree._version++;
            }
        }

        public bool TryGetValue([MaybeNullWhen(false)] out TValue value)
        {
            ref readonly var entry = ref EntryRef;
            if (entry.IsEnd) {
                value = entry.Value;
                return true;
            }
            else {
                value = default;
                return false;
            }
        }

        public ref TValue GetValueRefOrNullRef()
        {
            ref var entry = ref EntryRef;
            if (entry.IsEnd) {
                return ref entry.Value;
            }
            else {
                return ref Unsafe.NullRef<TValue>();
            }
        }

        public Node Parent
        {
            get {
                if (!HasKey)
                    return default;
                ref readonly var entry = ref EntryRef;
                if (entry.IsFreed)
                    return default;
                if (_index == 0) // is root
                    return default;
                return new Node(_tree, _index);
            }
        }

        public ChildNodesCollection Children => new(this);

        internal ref Entry EntryRef => ref _tree._entries[_index];

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
                if (!_node.HasKey)
                    return false;
                ref readonly var entry = ref _node.EntryRef;
                if (entry.IsFreed)
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
            if (!_node.HasKey)
                return Enumerator.Empty;
            ref readonly var entry = ref _node.EntryRef;
            if (entry.IsFreed)
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
            private readonly PrefixTreeDictionary<TKey, TValue> _tree;
            private readonly int _version;
            private int _index;
            private int _current;

            internal static Enumerator Empty => new(-1);

            internal Enumerator(PrefixTreeDictionary<TKey, TValue> tree, int firstSiblingIndex)
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
