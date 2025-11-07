using System.Collections;

namespace Trarizon.Library.Linq;

public static partial class TraTraversable
{
    public static TraversableValue<T> Create<T>(T root, Func<T, IEnumerable<T>> getChildren)
        => new TraversableValue<T>(root, getChildren);

    public readonly struct TraversableValue<T>(T value, Func<T, IEnumerable<T>> getChildren) : IChildrenTraversable<TraversableValue<T>>
    {
        public T Value => value;

        public ChildrenEnumerator GetChildrenEnumerator() => new(getChildren(value).GetEnumerator(), getChildren);

        IEnumerator<TraversableValue<T>> IChildrenTraversable<TraversableValue<T>>.GetChildrenEnumerator() => GetChildrenEnumerator();

        public readonly struct ChildrenEnumerator : IEnumerator<TraversableValue<T>>
        {
            private readonly IEnumerator<T> _enumerator;
            private readonly Func<T, IEnumerable<T>> _getChildren;

            internal ChildrenEnumerator(IEnumerator<T> enumerator, Func<T, IEnumerable<T>> getChildren)
            {
                _enumerator = enumerator;
                _getChildren = getChildren;
            }

            public TraversableValue<T> Current => new TraversableValue<T>(_enumerator.Current, _getChildren);

            object IEnumerator.Current => Current;

            public void Dispose() => _enumerator.Dispose();
            public bool MoveNext() => _enumerator.MoveNext();
            public void Reset() => _enumerator.Reset();
        }
    }
}
