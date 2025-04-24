using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Collections.Generic;
partial class PrefixTree<T>
{
    public Node RootNode
    {
        get {
            if (_entries.Length == 0)
                return default;
            return new Node(this, 0);
        }
    }

    public readonly struct Node : IEquatable<Node>
    {
        internal readonly PrefixTree<T> _tree;
        internal readonly int _index;

        internal Node(PrefixTree<T> tree, int index)
        {
            _tree = tree;
            _index = index;
        }

        public bool HasValue => _tree is not null;

        public bool IsEnd => EntryRef.IsEnd;

        public T Value
        {
            get => ValueRef;
            set {
                ValueRef = value;
                _tree._version++;
            }
        }

        public ref T ValueRef => ref EntryRef.Value;

        public Node Parent
        {
            get {
                if (!HasValue)
                    return default;
                ref readonly var entry = ref EntryRef;
                if (entry.IsFreed)
                    return default;
                if (_index == 0) // is root
                    return default;
                return new Node(_tree, _index);
            }
        }

        public ChildNodesEnumerable Children => new(this);

        internal ref Entry EntryRef => ref _tree._entries[_index];

        #region Equality

        public bool Equals(Node other) => _tree == other._tree && _index == other._index;
        public override bool Equals([NotNullWhen(true)] object? obj) => obj is Node node && Equals(node);
        public override int GetHashCode() => HashCode.Combine(_tree, _index);
        public static bool operator ==(Node left, Node right) => left.Equals(right);
        public static bool operator !=(Node left, Node right) => !(left == right);

        #endregion
    }

    public readonly struct ChildNodesEnumerable : IEnumerable<Node>
    {
        private readonly Node _node;

        internal ChildNodesEnumerable(Node node)
        {
            _node = node;
        }

        public readonly Enumerator GetEnumerator()
        {
            if (!_node.HasValue)
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
            private readonly PrefixTree<T> _tree;
            private readonly int _version;
            private int _index;
            private int _current;

            internal static Enumerator Empty => new(-1);

            internal Enumerator(PrefixTree<T> tree, int firstSiblingIndex)
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
                    TraThrow.CollectionModified();
            }

            void IEnumerator.Reset() => throw new NotImplementedException();
            readonly object? IEnumerator.Current => Current;
            readonly void IDisposable.Dispose() { }
        }
    }
}
