using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Library.Collections.Generic;
partial class PrefixTreeDictionary<TKey, TValue>
{
#if NET9_0_OR_GREATER

    public AlternateLookup<TAlternate> GetAlternateLookup<TAlternate>() where TAlternate : notnull, allows ref struct
    {
        if (!IsCompatibleAlternateKey<TAlternate>(this)) {
            Throws.IncompatibleAlternateComparer();
        }
        return new AlternateLookup<TAlternate>(this);
    }

    public bool TryGetAlternateLookup<TAlternate>(out AlternateLookup<TAlternate> lookup) where TAlternate : notnull, allows ref struct
    {
        if (!IsCompatibleAlternateKey<TAlternate>(this)) {
            lookup = default;
            return false;
        }
        lookup = new AlternateLookup<TAlternate>(this);
        return true;
    }

    internal static bool IsCompatibleAlternateKey<TAlternate>(PrefixTreeDictionary<TKey, TValue> tree) where TAlternate : allows ref struct
    {
        return tree._comparer is IAlternateEqualityComparer<TAlternate, TKey>;
    }

    public readonly struct AlternateLookup<TAlternate> where TAlternate : notnull, allows ref struct
    {
        internal AlternateLookup(PrefixTreeDictionary<TKey, TValue> tree)
        {
            // If comparer is null, typeof(T) is always value type, and the default comparer will never be IAlternateComaprer
            Debug.Assert(tree._comparer is not null);
            Debug.Assert(IsCompatibleAlternateKey<TAlternate>(tree));
            Tree = tree;
        }

        public PrefixTreeDictionary<TKey, TValue> Tree { get; }

        public IAlternateEqualityComparer<TAlternate, TKey> Comparer => Unsafe.As<IAlternateEqualityComparer<TAlternate, TKey>>(Tree._comparer!);

        public bool TryGetValue<TEnumerable>(TEnumerable sequence, [MaybeNullWhen(false)] out TValue value) where TEnumerable : IEnumerable<TAlternate>, allows ref struct
        {
            var node = FindPrefix(sequence);
            if (node is { IsEnd: true }) {
                value = node.GetValueOrDefault()!;
                return true;
            }
            else {
                value = default;
                return false;
            }
        }

        public bool TryGetNode<TEnumerable>(TEnumerable sequence, [MaybeNullWhen(false)] out Node node) where TEnumerable : IEnumerable<TAlternate>, allows ref struct
        {
            node = FindPrefix(sequence);
            return node is not null;
        }

        public bool ContainsKey<TEnumerable>(TEnumerable sequence) where TEnumerable : IEnumerable<TAlternate>, allows ref struct
        {
            var node = FindPrefix(sequence);
            if (node is null)
                return false;
            return node.IsEnd;
        }

        public bool ContainsKeyPrefix<TEnumerable>(TEnumerable sequence) where TEnumerable : IEnumerable<TAlternate>, allows ref struct
        {
            var node = FindPrefix(sequence);
            return node is not null;
        }

        public Node GetOrAdd<TEnumerable>(TEnumerable sequence) where TEnumerable : IEnumerable<TAlternate>, allows ref struct
        {
            var node = AddInternal(sequence);
            node.IsEnd = true;
            return node;
        }

        public bool TryAdd<TEnumerable>(TEnumerable sequence, out Node node) where TEnumerable : IEnumerable<TAlternate>, allows ref struct
        {
            node = AddInternal(sequence);
            if (node.IsEnd)
                return false;

            node.IsEnd = true;
            return true;
        }

        public bool Remove<TEnumerable>(TEnumerable sequence) where TEnumerable : IEnumerable<TAlternate>, allows ref struct
        {
            var node = FindPrefix(sequence);
            if (node is null)
                return false;
            if (!node.IsEnd)
                return false;

            Tree.RemoveInternal(node);
            return true;
        }

        private Node AddInternal<TEnumerable>(TEnumerable sequence) where TEnumerable : IEnumerable<TAlternate>, allows ref struct
        {
            var node = Tree.GetEnsuredRoot();
            foreach (var item in sequence) {
                node = GetOrAddChild(node, item);
            }
            Tree._version++;
            Tree._count++;
            return node;
        }

        internal Node GetOrAddChild(Node parent, TAlternate value)
        {
            Debug.Assert(IsCompatibleAlternateKey<TAlternate>(Tree));

            var child = parent._child;
            var comparer = Comparer;
            while (child is not null) {
                if (comparer.Equals(value, child.Key!))
                    return child;
                child = child._nextSibling;
            }

            child = new Node(Tree, comparer.Create(value));
            child._parent = parent;
            child._nextSibling = parent._child;
            parent._child = child;
            Tree._nodeCount++;
            return child;
        }

        internal Node? FindPrefix<TEnumerable>(TEnumerable prefix)
            where TEnumerable : IEnumerable<TAlternate>, allows ref struct
        {
            Debug.Assert(IsCompatibleAlternateKey<TAlternate>(Tree));

            if (Tree._root is null)
                return null;
            var node = Tree._root;

            var comparer = Comparer;
            foreach (var val in prefix) {
                var child = node._child;
                while (child is not null) {
                    if (comparer.Equals(val, child.Key!)) {
                        node = child;
                        goto ContinueFor;
                    }
                    child = child._nextSibling;
                }
                return null;

            ContinueFor:
                continue;
            }

            return node;
        }
    }

#endif
}

public static partial class PrefixTreeExtensions
{
#if NET9_0_OR_GREATER

    public static bool TryGetValue<TKey, TValue, TAlternate>(this PrefixTreeDictionary<TKey, TValue>.AlternateLookup<TAlternate> lookup, ReadOnlySpan<TAlternate> sequence, [MaybeNullWhen(false)] out TValue value)
         where TKey : notnull where TAlternate : notnull
    {
        var node = FindPrefix(lookup, sequence);
        if (node is { IsEnd: true }) {
            value = node.GetValueOrDefault()!;
            return true;
        }
        else {
            value = default;
            return false;
        }
    }

    public static bool TryGetNode<TKey, TValue, TAlternate>(this PrefixTreeDictionary<TKey, TValue>.AlternateLookup<TAlternate> lookup, ReadOnlySpan<TAlternate> sequence, [MaybeNullWhen(false)] out PrefixTreeDictionary<TKey, TValue>.Node node)
        where TKey : notnull where TAlternate : notnull
    {
        node = FindPrefix(lookup, sequence);
        return node is not null;
    }

    public static bool ContainsKey<TKey, TValue, TAlternate>(this PrefixTreeDictionary<TKey, TValue>.AlternateLookup<TAlternate> lookup, ReadOnlySpan<TAlternate> sequence)
        where TKey : notnull where TAlternate : notnull
    {
        var node = FindPrefix(lookup, sequence);
        if (node is null)
            return false;
        return node.IsEnd;
    }

    public static bool ContainsKeyPrefix<TKey, TValue, TAlternate>(this PrefixTreeDictionary<TKey, TValue>.AlternateLookup<TAlternate> lookup, ReadOnlySpan<TAlternate> sequence)
        where TKey : notnull where TAlternate : notnull
    {
        var node = FindPrefix(lookup, sequence);
        return node is not null;
    }

    public static PrefixTreeDictionary<TKey, TValue>.Node GetOrAdd<TKey, TValue, TAlternate>(this PrefixTreeDictionary<TKey, TValue>.AlternateLookup<TAlternate> lookup, ReadOnlySpan<TAlternate> sequence)
        where TKey : notnull where TAlternate : notnull
    {
        var node = AddInternal(lookup, sequence);
        node.IsEnd = true;
        return node;
    }

    public static bool TryAdd<TKey, TValue, TAlternate>(this PrefixTreeDictionary<TKey, TValue>.AlternateLookup<TAlternate> lookup, ReadOnlySpan<TAlternate> sequence, out PrefixTreeDictionary<TKey, TValue>.Node node)
        where TKey : notnull where TAlternate : notnull
    {
        node = AddInternal(lookup, sequence);
        if (node.IsEnd)
            return false;

        node.IsEnd = true;
        return true;
    }

    public static bool Remove<TKey, TValue, TAlternate>(this PrefixTreeDictionary<TKey, TValue>.AlternateLookup<TAlternate> lookup, ReadOnlySpan<TAlternate> sequence)
        where TKey : notnull where TAlternate : notnull
    {
        var node = FindPrefix(lookup, sequence);
        if (node is null)
            return false;
        if (!node.IsEnd)
            return false;

        lookup.Tree.RemoveInternal(node);
        return true;
    }

    private static PrefixTreeDictionary<TKey, TValue>.Node AddInternal<TKey, TValue, TAlternate>(this PrefixTreeDictionary<TKey, TValue>.AlternateLookup<TAlternate> lookup, ReadOnlySpan<TAlternate> sequence)
        where TKey : notnull where TAlternate : notnull
    {
        var tree = lookup.Tree;
        var node = tree.Root;
        foreach (var item in sequence) {
            node = lookup.GetOrAddChild(node, item);
        }
        tree._version++;
        tree._count++;
        return node;
    }

    private static PrefixTreeDictionary<TKey, TValue>.Node? FindPrefix<TKey, TValue, TAlternate>(this PrefixTreeDictionary<TKey, TValue>.AlternateLookup<TAlternate> lookup, ReadOnlySpan<TAlternate> prefix)
        where TKey : notnull where TAlternate : notnull
    {
        Debug.Assert(PrefixTreeDictionary<TKey, TValue>.IsCompatibleAlternateKey<TAlternate>(lookup.Tree));

        var tree = lookup.Tree;
        if (tree._root is null)
            return null;
        var node = tree._root;

        var comparer = lookup.Comparer;
        foreach (var val in prefix) {
            var child = node._child;
            while (child is not null) {
                if (comparer.Equals(val, child.Key!)) {
                    node = child;
                    goto ContinueFor;
                }
                child = child._nextSibling;
            }
            return null;

        ContinueFor:
            continue;
        }

        return node;
    }

#endif
}
