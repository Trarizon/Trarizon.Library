using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Collections.Generic;

#if NET9_0_OR_GREATER

public static class PrefixTreeExtensions
{
    extension<T, TAlternate>(PrefixTree<T>.AlternateLookup<TAlternate> lookup)
    {
        public bool TryGetNode(ReadOnlySpan<TAlternate> prefixSequence, [MaybeNullWhen(false)] out PrefixTree<T>.Node node)
        {
            node = lookup.FindPrefixNode(prefixSequence);
            return node is not null;
        }

        public bool Contains(ReadOnlySpan<TAlternate> sequence)
        {
            var node = lookup.FindPrefixNode(sequence);
            if (node is null)
                return false;
            return node.IsEnd;
        }

        public bool ContainsPrefix(ReadOnlySpan<TAlternate> prefixSequence)
            => lookup.FindPrefixNode(prefixSequence) is not null;

        private PrefixTree<T>.Node? FindPrefixNode(ReadOnlySpan<TAlternate> prefix)
        {
            Debug.Assert(PrefixTree<T>.IsCompatibleAlternateKey<TAlternate>(lookup.Tree));
            var tree = lookup.Tree;
            if (tree._root is null)
                return null;
            var node = tree._root;

            var comparer = lookup.Comparer;
            foreach (var val in prefix) {
                var children = node.ChildrenSpan;
                foreach (var child in children) {
                    if (comparer.Equals(val, child.Value!)) {
                        node = child;
                        goto ContinueFor;
                    }
                }
                return null;

            ContinueFor:
                continue;
            }

            return node;
        }

        public PrefixTree<T>.Node GetOrAdd(ReadOnlySpan<TAlternate> sequence)
        {
            lookup.TryAdd(sequence, out var node);
            return node;
        }

        public bool TryAdd(ReadOnlySpan<TAlternate> sequence, out PrefixTree<T>.Node node)
        {
            var tree = lookup.Tree;
            node = tree.Root;
            foreach (var item in sequence) {
                node = lookup.GetOrAddChild(node, item);
            }

            if (node._end) {
                return false;
            }
            else {
                tree._version++;
                tree._count++;
                node._end = true;
                return true;
            }
        }

        public bool Remove(ReadOnlySpan<TAlternate> sequence)
        {
            var node = lookup.FindPrefixNode(sequence);
            if (node is null)
                return false;
            if (!node._end)
                return false;

            var tree = lookup.Tree;
            tree.RemoveEndNode(node);
            tree._count--;
            tree._version++;
            return true;
        }
    }

    extension<TKey, TValue, TAlternate>(PrefixTreeDictionary<TKey, TValue>.AlternateLookup<TAlternate> lookup) where TKey : notnull where TAlternate : notnull
    {
        public bool TryGetValue(ReadOnlySpan<TAlternate> sequence, [MaybeNullWhen(false)] out TValue value)
        {
            var node = lookup.FindPrefixNode(sequence);
            if (node is { IsEnd: true }) {
                value = node.GetValueOrDefault()!;
                return true;
            }
            else {
                value = default;
                return false;
            }
        }

        public bool TryGetNode(ReadOnlySpan<TAlternate> prefixSequence, [MaybeNullWhen(false)] out PrefixTreeDictionary<TKey, TValue>.Node node)
        {
            node = lookup.FindPrefixNode(prefixSequence);
            return node is not null;
        }

        public bool Contains(ReadOnlySpan<TAlternate> sequence)
        {
            var node = lookup.FindPrefixNode(sequence);
            if (node is null)
                return false;
            return node.IsEnd;
        }

        public bool ContainsPrefix(ReadOnlySpan<TAlternate> prefixSequence)
            => lookup.FindPrefixNode(prefixSequence) is not null;

        private PrefixTreeDictionary<TKey, TValue>.Node? FindPrefixNode(ReadOnlySpan<TAlternate> prefix)
        {
            Debug.Assert(PrefixTreeDictionary<TKey, TValue>.IsCompatibleAlternateKey<TAlternate>(lookup.Tree));
            var tree = lookup.Tree;
            if (tree._root is null)
                return null;
            var node = tree._root;

            var comparer = lookup.Comparer;
            foreach (var val in prefix) {
                var children = node.ChildrenSpan;
                foreach (var child in children) {
                    if (comparer.Equals(val, child.Key!)) {
                        node = child;
                        goto ContinueFor;
                    }
                }
                return null;

            ContinueFor:
                continue;
            }

            return node;
        }

        public PrefixTreeDictionary<TKey, TValue>.Node GetOrAdd(ReadOnlySpan<TAlternate> sequence, TValue value)
        {
            lookup.TryAdd(sequence, value, out var node);
            return node;
        }

        public bool TryAdd(ReadOnlySpan<TAlternate> sequence, TValue value, out PrefixTreeDictionary<TKey, TValue>.Node node)
        {
            var tree = lookup.Tree;
            node = tree.Root;
            foreach (var item in sequence) {
                node = lookup.GetOrAddChild(node, item);
            }

            if (node.IsEnd) {
                return false;
            }
            else {
                tree._version++;
                tree._count++;
                node.SetIsEnd(value);
                return true;
            }
        }

        public bool Remove(ReadOnlySpan<TAlternate> sequence)
        {
            var node = lookup.FindPrefixNode(sequence);
            if (node is null)
                return false;
            if (!node.IsEnd)
                return false;

            var tree = lookup.Tree;
            tree.RemoveEndNode(node);
            tree._count--;
            tree._version++;
            return true;
        }
    }
}

#endif
