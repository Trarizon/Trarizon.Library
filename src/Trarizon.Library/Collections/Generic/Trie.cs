using System.Diagnostics;
using Trarizon.Library.Collections.AllocOpt;

namespace Trarizon.Library.Collections.Generic;
public class Trie<T>
{
    private AllocOptList<Entry> _items;

    private ref Entry RootEntry => ref _items.GetUnderlyingArray()[0];

    private Trie()
    {
    }

    private void EnsureInitialized()
    {
        if (_items.GetUnderlyingArray() is null or [])
            _items = [new Entry()];
    }

    public void Add(ReadOnlySpan<T> values)
    {
        EnsureInitialized();
        ref var entry = ref RootEntry;
        foreach (var value in values) {
            entry = ref AddChild(ref entry, value);
        }
        entry.HasEnd = true;
    }

    private ref Entry AddChild(ref Entry entry, T value)
    {
        Debug.Assert(GetEntryIndex(in entry) < _items.Count);

        if (entry.ChildIndex == -1) {
            entry.ChildIndex = _items.Count;
            _items.Add(new(value) { ParentIndex = GetEntryIndex(in entry), });
            return ref _items.AtRef(entry.ChildIndex);
        }

        ref var child = ref _items.AtRef(entry.ChildIndex);
        while (true) {
            if (child.NextSiblingIndex == -1) {
                entry.NextSiblingIndex = _items.Count;
                _items.Add(new(value) { ParentIndex = child.ParentIndex, });
                return ref _items.AtRef(entry.NextSiblingIndex);
            }

            child = ref _items.AtRef(child.NextSiblingIndex);
            if (EqualityComparer<T>.Default.Equals(child.Value, value)) {
                return ref child;
            }
        }
    }

    private int GetEntryIndex(ref readonly Entry entry) => _items.GetUnderlyingArray().OffsetOf(in entry);

    public struct Entry(T value)
    {
        public T Value = value;
        internal int ParentIndex = -1;
        internal int ChildIndex = -1;
        internal int NextSiblingIndex = -1;
        internal bool HasEnd;
    }

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
}
