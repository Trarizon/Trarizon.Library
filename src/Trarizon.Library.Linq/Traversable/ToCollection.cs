namespace Trarizon.Library.Linq;

public static partial class TraTraversable
{
    public static IChildrenTraversable<T> AsChildrenTraversable<T>(this IChildrenTraversable<T> source)        => source;

    public static IEnumerable<T> EnumerateChildren<T>(this IChildrenTraversable<T> source)
        => new ChildrenEnumerable<T>(source);

    private sealed class ChildrenEnumerable<T>(IChildrenTraversable<T> source) : TraEnumerable.IteratorBase<T>
    {
        private IEnumerator<T> _enumerator = default!;

        public override T Current => _enumerator.Current;

        public override bool MoveNext()
        {
            switch (_state) {
                case InitState:
                    _state = 0;
                    _enumerator = source.GetChildrenEnumerator();
                    goto default;
                case MinPreservedState - 1:
                    return false;
                default:
                    var res = _enumerator.MoveNext();
                    if (!res) _state = MinPreservedState - 1;
                    return res;
            }
        }
        protected override TraEnumerable.IteratorBase<T> Clone()
            => new ChildrenEnumerable<T>(source);

        protected override void DisposeInternal() => _enumerator.Dispose();
    }
}
