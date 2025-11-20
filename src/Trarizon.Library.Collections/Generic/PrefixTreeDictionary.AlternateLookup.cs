using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Library.Collections.Generic;

#if NET9_0_OR_GREATER

partial class PrefixTreeDictionary<TKey, TValue>
{
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
        => tree._comparer is IAlternateEqualityComparer<TAlternate, TKey>;

    public readonly struct AlternateLookup<TAlternate> where TAlternate : notnull, allows ref struct
    {
        public PrefixTreeDictionary<TKey, TValue> Tree { get; }

        public IAlternateEqualityComparer<TAlternate, TKey> Comparer => Unsafe.As<IAlternateEqualityComparer<TAlternate, TKey>>(Tree._comparer!);

        public AlternateLookup(PrefixTreeDictionary<TKey, TValue> tree)
        {
            // If comparer is null, typeof(T) is always value type, and the default comparer will never be IAlternateComaprer
            Debug.Assert(tree._comparer is not null);
            Debug.Assert(tree._comparer is IAlternateEqualityComparer<TAlternate, TKey>);
            Tree = tree;
        }

        public bool TryGetValue(IEnumerable<TAlternate> sequence, [MaybeNullWhen(false)] out TValue value)
        {
            var node = FindPrefixNode(sequence);
            if (node is { IsEnd: true }) {
                value = node.GetValueOrDefault()!;
                return true;
            }
            else {
                value = default;
                return false;
            }
        }

        public bool TryGetNode(IEnumerable<TAlternate> prefixSequence, [MaybeNullWhen(false)] out Node node)
        {
            node = FindPrefixNode(prefixSequence);
            return node is not null;
        }

        public bool ContainsKey(IEnumerable<TAlternate> sequence)
            => FindPrefixNode(sequence) is { IsEnd: true };

        public bool ContainsKeyPrefix(IEnumerable<TAlternate> prefixSequence)
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

        public Node GetOrAdd(IEnumerable<TAlternate> sequence, TValue value)
        {
            TryAdd(sequence, value, out var node);
            return node;
        }

        public bool TryAdd(IEnumerable<TAlternate> sequence, TValue value, out Node node)
        {
            node = Tree.Root;
            foreach (var item in sequence) {
                node = GetOrAddChild(node, item);
            }

            if (node.IsEnd) {
                return false;
            }
            Tree._version++;
            Tree._count++;
            node.SetIsEnd(value);
            return true;
        }

        internal Node GetOrAddChild(Node parent, TAlternate value)
        {
            var children = parent.ChildrenSpan;
            var comparer = Comparer;
            foreach (var child in children) {
                if (comparer.Equals(value, child.Key!))
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
