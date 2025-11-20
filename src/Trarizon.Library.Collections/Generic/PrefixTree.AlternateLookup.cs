using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Library.Collections.Generic;

#if NET9_0_OR_GREATER

partial class PrefixTree<T>
{
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
        => tree._comparer is IAlternateEqualityComparer<TAlternate, T>;

    public readonly struct AlternateLookup<TAlternate> where TAlternate : allows ref struct
    {
        public PrefixTree<T> Tree { get; }

        public IAlternateEqualityComparer<TAlternate, T> Comparer => Unsafe.As<IAlternateEqualityComparer<TAlternate, T>>(Tree._comparer!);

        public AlternateLookup(PrefixTree<T> tree)
        {
            // If comparer is null, typeof(T) is always value type, and the default comparer will never be IAlternateComaprer
            Debug.Assert(tree._comparer is not null);
            Debug.Assert(tree._comparer is IAlternateEqualityComparer<TAlternate, T>);
            Tree = tree;
        }

        public bool TryGetNode(IEnumerable<TAlternate> prefixSequence, [MaybeNullWhen(false)] out Node node)
        {
            node = FindPrefixNode(prefixSequence);
            return node is not null;
        }

        public bool Contains(IEnumerable<TAlternate> sequence)
            => FindPrefixNode(sequence) is { IsEnd: true };

        public bool ContainsPrefix(IEnumerable<TAlternate> prefixSequence)
            => FindPrefixNode(prefixSequence) is not null;

        private Node? FindPrefixNode(IEnumerable<TAlternate> prefix)
        {
            var node = Tree._root;
            if (node is null)
                return null;

            var comparer = Comparer;
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

        public Node GetOrAdd(IEnumerable<TAlternate> sequence)
        {
            TryAdd(sequence, out var node);
            return node;
        }

        public bool TryAdd(IEnumerable<TAlternate> sequence, out Node node)
        {
            node = Tree.Root;
            foreach (var item in sequence) {
                node = GetOrAddChild(node, item);
            }

            if (node._end) {
                return false;
            }

            Tree._version++;
            Tree._count++;
            node._end = true;
            return true;
        }

        internal Node GetOrAddChild(Node parent, TAlternate value)
        {
            var children = parent.ChildrenSpan;
            var comparer = Comparer;
            foreach (var child in children) {
                if (comparer.Equals(value, child.Value!))
                    return child;
            }

            var node = new Node(Tree, comparer.Create(value));
            parent.AddChildAndSetParent(node);
            Tree._nodeCount++;
            return node;
        }

        public bool Remove(IEnumerable<TAlternate> sequence)
        {
            var node = FindPrefixNode(sequence);
            if (node is not { IsEnd: true })
                return false;

            Tree.RemoveEndNode(node);
            Tree._count--;
            Tree._version++;
            return true;
        }

        public void Clear() => Tree.Clear();
    }
}

#endif
