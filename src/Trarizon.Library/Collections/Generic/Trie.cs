using CommunityToolkit.HighPerformance;
using System.Diagnostics;

namespace Trarizon.Library.Collections.Generic;
public class Trie<T>
{
    internal List<Entry> _items;
    private IEqualityComparer<T>? _comparer;

    private ref Entry RootEntry => ref _items.AsSpan()[0];

    public Trie(IEqualityComparer<T>? comparer)
    {
        _items = [new Entry(default!, -1)];
        _comparer = comparer;
    }

    public Trie()
    {
        _items = [new Entry(default!, -1)];
        _comparer = null;
    }

    public bool Contains(ReadOnlySpan<T> values)
    {
        var entry = RootEntry;
        var items = _items.AsSpan();
        foreach (var val in values) {
            if (entry.ChildIndex == -1)
                return false;
            var child = items[entry.ChildIndex];
            while (!EqualityComparer<T>.Default.Equals(child.Value, val)) {
                if (child.NextSiblingIndex == -1)
                    return false;
                child = items[child.NextSiblingIndex];
            }
            entry = child;
        }
        return entry.HasEnd;
    }

    public void Add(ReadOnlySpan<T> values)
    {
        ref var entry = ref RootEntry;
        foreach (var value in values) {
            entry = ref GetOrAddChild(ref entry, value);
        }
        entry.HasEnd = true;
    }


    private ref Entry GetOrAddChild(ref Entry parent, T value)
    {
        Debug.Assert(GetEntryIndex(in parent) < _items.Count);

        // parent has no child, this is the firs child
        if (parent.ChildIndex == -1) {
            parent.ChildIndex = _items.Count;
            _items.Add(new Entry(value, GetEntryIndex(in parent)));
            return ref _items.AsSpan()[parent.ChildIndex];
        }

        ref var child = ref _items.AsSpan()[parent.ChildIndex];

        if (typeof(T).IsValueType && _comparer is null) {
            while (true) {
                // Exists, return the existing value
                if (EqualityComparer<T>.Default.Equals(value, child.Value)) {
                    return ref child;
                }

                if (child.NextSiblingIndex == -1)
                    break;
                child = ref _items.AsSpan()[child.NextSiblingIndex];
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
                child = ref _items.AsSpan()[child.NextSiblingIndex];
            }
        }

        // Add as sibling
        child.NextSiblingIndex = _items.Count;
        _items.Add(new Entry(value, child.ParentIndex));
        return ref _items.AsSpan()[child.NextSiblingIndex];
    }

    private int GetEntryIndex(ref readonly Entry entry)
        => _items.AsSpan().OffsetOf(in entry);


    /*
         public IEnumerable<(T Value, bool HasEnd)> DFS()
    {
        if (_items.GetUnderlyingArray() is null or [])
            yield break;

        AllocOptStack<int> stack = [RootEntry.ChildIndex];

        while (stack.Count > 0) {
            var entry = _items[stack.Peek()];
            yield return (entry.Value, entry.HasEnd);
            if (entry.ChildIndex != -1) {
                stack.Push(entry.ChildIndex);
            }
            else if (entry.NextSiblingIndex != -1) {
                stack.Push(entry.NextSiblingIndex);
            }
            else {
                stack.Pop();
            }
        }
    }

     */

    internal struct Entry(T value, int parentIndex)
    {
        public T Value = value;
        public int ParentIndex = parentIndex;
        public int ChildIndex = -1; // Index of first child, -1 means no child
        public int NextSiblingIndex = -1; // Index of next sibling, -1 means last sibling
        public bool HasEnd;
    }

    public readonly struct Builder
    {
        private readonly Trie<T> _trie;

        public void Add(ReadOnlySpan<T> values)
        {
            ref var entry = ref _trie.RootEntry;
            foreach (var value in values) {
                entry = ref _trie.GetOrAddChild(ref entry, value);
            }
            entry.HasEnd = true;
        }

        public void Add(T[] values) => Add(values.AsSpan());

        public void Add(IEnumerable<T> values)
        {
            ref var entry = ref _trie.RootEntry;
            foreach (var value in values) {
                entry = ref _trie.GetOrAddChild(ref entry, value);
            }
            entry.HasEnd = true;
        }
    }
}

public static class TrieExt
{
    public static IEnumerator<string> GetEnumerator(this Trie<char> trie)
    {
        var items = trie._items;
        Stack<char> chars = new Stack<char>();
        var entry = items[0];
        while (true) {
            if (entry.ChildIndex != -1) {
                entry = items[entry.ChildIndex];
                chars.Push(entry.Value);
                if (entry.HasEnd)
                    yield return chars.Reverse().ToArray().AsSpan().ToString()!;
                continue;
            }

            while (true) {
                chars.Pop();
                if (entry.NextSiblingIndex != -1) {
                    entry = items[entry.NextSiblingIndex];
                    chars.Push(entry.Value);
                    if (entry.HasEnd)
                        yield return chars.ToArray().AsSpan().ToString()!;
                    break;
                }

                if (chars.Count == 0) {
                    yield break;
                }
                else {
                    entry = items[entry.ParentIndex]; // = chars.peek()
                    continue;
                }
            }
        }
    }
}