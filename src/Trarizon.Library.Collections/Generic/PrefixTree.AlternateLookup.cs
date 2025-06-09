using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Library.Collections.Generic;
partial class PrefixTree<T>
{
#if NET9_0_OR_GREATER

    public AlternateLookup<TAlternate> GetAlternateLookup<TAlternate>() where TAlternate : allows ref struct
    {
        if (!IsCompatibleAlternateKey<TAlternate>(this)) {
            Throws.IncompatibleAlternateComparer();
        }
        return new AlternateLookup<TAlternate>(this);
    }

    public bool TryGetAlternateLookup<TAlternate>(out AlternateLookup<TAlternate> lookup) where TAlternate : allows ref struct
    {
        if (!IsCompatibleAlternateKey<TAlternate>(this)) {
            lookup = default;
            return false;
        }
        lookup = new AlternateLookup<TAlternate>(this);
        return true;
    }

    internal static bool IsCompatibleAlternateKey<TAlternate>(PrefixTree<T> tree) where TAlternate : allows ref struct
    {
        return tree._comparer is IAlternateEqualityComparer<TAlternate, T>;
    }

    public readonly struct AlternateLookup<TAlternate> where TAlternate : allows ref struct
    {
        internal AlternateLookup(PrefixTree<T> tree)
        {
            // If comparer is null, typeof(T) is always value type, and the default comparer will never be IAlternateComaprer
            Debug.Assert(tree._comparer is not null);
            Debug.Assert(IsCompatibleAlternateKey<TAlternate>(tree));
            Tree = tree;
        }

        public PrefixTree<T> Tree { get; }

        public IAlternateEqualityComparer<TAlternate, T> Comparer => Unsafe.As<IAlternateEqualityComparer<TAlternate, T>>(Tree._comparer!);

        public bool TryGetNode<TEnumerable>(TEnumerable sequence, [MaybeNullWhen(false)] out Node node) where TEnumerable : IEnumerable<TAlternate>, allows ref struct
        {
            node = FindPrefix(sequence);
            return node is not null;
        }

        public bool Contains<TEnumerable>(TEnumerable sequence) where TEnumerable : IEnumerable<TAlternate>, allows ref struct
        {
            var node = FindPrefix(sequence);
            if (node is null)
                return false;
            return node.IsEnd;
        }

        public bool ContainsPrefix<TEnumerable>(TEnumerable sequence) where TEnumerable : IEnumerable<TAlternate>, allows ref struct
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
                if (comparer.Equals(value, child.Value!))
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
                    if (comparer.Equals(val, child.Value!)) {
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

    public static bool TryGetNode<T, TAlternate>(this PrefixTree<T>.AlternateLookup<TAlternate> lookup, ReadOnlySpan<TAlternate> sequence, [MaybeNullWhen(false)] out PrefixTree<T>.Node node)
    {
        node = FindPrefix(lookup, sequence);
        return node is not null;
    }

    public static bool Contains<T, TAlternate>(this PrefixTree<T>.AlternateLookup<TAlternate> lookup, ReadOnlySpan<TAlternate> sequence)
    {
        var node = FindPrefix(lookup, sequence);
        if (node is null)
            return false;
        return node.IsEnd;
    }

    public static bool ContainsPrefix<T, TAlternate>(this PrefixTree<T>.AlternateLookup<TAlternate> lookup, ReadOnlySpan<TAlternate> sequence)
    {
        var node = FindPrefix(lookup, sequence);
        return node is not null;
    }

    public static PrefixTree<T>.Node GetOrAdd<T, TAlternate>(this PrefixTree<T>.AlternateLookup<TAlternate> lookup, ReadOnlySpan<TAlternate> sequence)
    {
        var node = AddInternal(lookup, sequence);
        node.IsEnd = true;
        return node;
    }

    public static bool TryAdd<T, TAlternate>(this PrefixTree<T>.AlternateLookup<TAlternate> lookup, ReadOnlySpan<TAlternate> sequence, out PrefixTree<T>.Node node)
    {
        node = AddInternal(lookup, sequence);
        if (node.IsEnd)
            return false;

        node.IsEnd = true;
        return true;
    }

    public static bool Remove<T, TAlternate>(this PrefixTree<T>.AlternateLookup<TAlternate> lookup, ReadOnlySpan<TAlternate> sequence)
    {
        var node = FindPrefix(lookup, sequence);
        if (node is null)
            return false;
        if (!node.IsEnd)
            return false;

        lookup.Tree.RemoveInternal(node);
        return true;
    }

    private static PrefixTree<T>.Node AddInternal<T, TAlternate>(PrefixTree<T>.AlternateLookup<TAlternate> lookup, ReadOnlySpan<TAlternate> sequence)
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

    private static PrefixTree<T>.Node? FindPrefix<T, TAlternate>(PrefixTree<T>.AlternateLookup<TAlternate> lookup, ReadOnlySpan<TAlternate> prefix)
    {
        Debug.Assert(PrefixTree<T>.IsCompatibleAlternateKey<TAlternate>(lookup.Tree));

        var tree = lookup.Tree;
        if (tree._root is null)
            return null;
        var node = tree._root;

        var comparer = lookup.Comparer;
        foreach (var val in prefix) {
            var child = node._child;
            while (child is not null) {
                if (comparer.Equals(val, child.Value!)) {
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