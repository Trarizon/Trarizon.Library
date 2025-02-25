using CommunityToolkit.HighPerformance;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
#if NETSTANDARD
using Unsafe = Trarizon.Library.Netstd.NetstdFix_Unsafe;
#endif

namespace Trarizon.Library.Collections.Generic;
public static class Trie
{
#if NET9_0_OR_GREATER
    [OverloadResolutionPriority(-1)]
#endif
    public static Trie<char> Create(IEnumerable<string> strings)
    {
        var trie = new Trie<char>();
        foreach (var str in strings) {
            trie.Add(str.AsSpan());
        }
        return trie;
    }

    public static Trie<char> Create(params ReadOnlySpan<string> strings)
    {
        var trie = new Trie<char>();
        foreach (var str in strings) {
            trie.Add(str.AsSpan());
        }
        return trie;
    }
}

public class Trie<T>
{
    private Entry[] _entries;
    private int _count;
    private IEqualityComparer<T>? _comparer;

    /// <summary>
    /// Root entry represents an empty sequence
    /// </summary>
    internal Node RootNode => _count >= 0 ? new(this, 0) : new(this, -1);

    public Trie(IEqualityComparer<T>? comparer)
    {
        _entries = [];
        _comparer = comparer;
    }

    public Trie()
    {
        _entries = [];
        _comparer = null;
    }

    public bool Contains(ReadOnlySpan<T> values)
    {
        ref readonly var entry = ref FindPrefixEndEntry(values);
        if (Unsafe.IsNullRef(in entry))
            return false;
        return entry.IsEnd;
    }

    public bool ContainsPrefix(ReadOnlySpan<T> prefix)
    {
        ref readonly var entry = ref FindPrefixEndEntry(prefix);
        if (Unsafe.IsNullRef(in entry))
            return false;
        return true;
    }

    private ref readonly Entry FindPrefixEndEntry(ReadOnlySpan<T> prefix)
    {
        if (_entries.Length == 0)
            return ref Unsafe.NullRef<Entry>();

        ref readonly var entry = ref _entries[0];

        if (typeof(T).IsValueType && _comparer is null) {
            foreach (var val in prefix) {
                if (entry.ChildIndex == -1)
                    return ref Unsafe.NullRef<Entry>();

                entry = ref _entries[entry.ChildIndex];
                while (!EqualityComparer<T>.Default.Equals(entry.Value, val)) {
                    if (entry.NextSiblingIndex == -1)
                        return ref Unsafe.NullRef<Entry>();
                    entry = ref _entries[entry.NextSiblingIndex];
                }
            }
        }
        else {
            var comparer = _comparer ??= EqualityComparer<T>.Default;
            foreach (var val in prefix) {
                if (entry.ChildIndex == -1)
                    return ref Unsafe.NullRef<Entry>();

                entry = ref _entries[entry.ChildIndex];
                while (!comparer.Equals(entry.Value, val)) {
                    if (entry.NextSiblingIndex == -1)
                        return ref Unsafe.NullRef<Entry>();
                    entry = ref _entries[entry.NextSiblingIndex];
                }
            }
        }
        return ref entry;
    }

    public bool Add(ReadOnlySpan<T> values)
    {
        if (_count == 0) {
            AddEntry(new Entry(default!, -1));
        }
        ref var entry = ref _entries[0];

        foreach (var value in values) {
            entry = ref GetOrAddChild(ref entry, value);
        }
        var exists = entry.IsEnd;
        entry.IsEnd = true;
        return !exists;
    }

    private ref Entry GetOrAddChild(ref Entry parent, T value)
    {
        Debug.Assert(DangerousGetEntryIndex(in parent) < _count);
        // parent has no child, this is the first child
        if (parent.ChildIndex == -1) {
            parent.ChildIndex = _count;
            AddEntry(new Entry(value, DangerousGetEntryIndex(in parent)));
            return ref _entries[parent.ChildIndex];
        }
        ref var child = ref _entries[parent.ChildIndex];

        if (typeof(T).IsValueType && _comparer is null) {
            while (true) {
                // Exists, return the existing value
                if (EqualityComparer<T>.Default.Equals(value, child.Value)) {
                    return ref child;
                }

                if (child.NextSiblingIndex == -1)
                    break;
                child = ref _entries[child.NextSiblingIndex];
            }
        }
        else {
            var comparer = _comparer ??= EqualityComparer<T>.Default;
            while (true) {
                if (comparer.Equals(value, child.Value)) {
                    return ref child;
                }

                if (child.NextSiblingIndex == -1)
                    break;
                child = ref _entries[child.NextSiblingIndex];
            }
        }

        // Add as sibling
        child.NextSiblingIndex = _count;
        AddEntry(new Entry(value, child.ParentIndex));
        return ref _entries[child.NextSiblingIndex];
    }

    private int DangerousGetEntryIndex(ref readonly Entry entry)
        => _entries.AsSpan().OffsetOf(in entry);

    private void AddEntry(Entry entry)
    {
        if (_count == _entries.Length) {
            ArrayGrowHelper.Grow(ref _entries, _count + 1, _count);
        }

        _entries[_count] = entry;
        _count++;
    }

    private struct Entry(T value, int parentIndex)
    {
        public T Value = value;
        public int ParentIndex = parentIndex;
        public int ChildIndex = -1; // Index of first child, -1 means no child
        public int NextSiblingIndex = -1; // Index of next sibling, -1 means last sibling
        public bool IsEnd;
    }

    internal readonly struct Node
    {
        private readonly Trie<T> _trie;
        private readonly int _index;

        public bool HasValue => _index >= 0;

        internal Node(Trie<T> trie, int index)
        {
            _trie = trie;
            _index = index;
        }

        public T Value => Entry.Value;

        /// <summary>
        /// This node is end of a <typeparamref name="T"/> sequence. Note that there may still be child of a "end" node
        /// </summary>
        public bool IsEnd => Entry.IsEnd;

        private ref readonly Entry Entry => ref _trie._entries[_index];

        public Node Parent => new(_trie, Entry.ParentIndex);

        public Node Child => new(_trie, Entry.ChildIndex);

        public Node NextSibling => new(_trie, Entry.NextSiblingIndex);
    }
}

public static class TrieExt
{
    [Experimental("TRAEXP")]
    public static IEnumerator<string> GetEnumerator(this Trie<char> trie)
    {
        var node = trie.RootNode;
        if (!node.HasValue)
            yield break;
        if (node.IsEnd)
            yield return "";
        int depth = 0;

        while (true) {
            if (node.Child is { HasValue: true } child) {
                node = child;
                depth++;
                if (node.IsEnd)
                    yield return ToString(node, depth);
                continue;
            }

            while (true) {
                if (node.NextSibling is { HasValue: true } sibling) {
                    node = sibling;
                    if (sibling.IsEnd)
                        yield return ToString(node, depth);
                    break; // continue outer while
                }

                if (node.Parent is { HasValue: true } parent) {
                    node = node.Parent;
                    depth--;
                    continue; // search next sibling
                }

                // Node is the last sibling and the root
                yield break;
            }
        }

        static string ToString(Trie<char>.Node node, int depth)
        {
            Debug.Assert(node.HasValue && node.IsEnd);
            Span<char> chars = stackalloc char[depth];
            for (int i = chars.Length - 1; i >= 0; i--) {
                ref var c = ref chars[i];
                Debug.Assert(node.HasValue);
                c = node.Value;
                node = node.Parent;
            }
            return chars.ToString();
        }
    }
}