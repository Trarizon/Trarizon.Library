using System.Collections;

namespace Trarizon.Library.Collections.Extensions.Query.Queriers;
internal abstract class EnumerationQuerier<T> : IEnumerator<T>, IEnumerable<T>
{
    protected const int MinPreservedState = -2;
    protected static readonly EnumerationQuerier<T> Empty = new EmptyQuerier();

    /// <summary>
    /// This value is -1 before first MoveNext(),
    /// -2 is preserved state, do not use it.
    /// </summary>
    protected int _state;
    private readonly int _threadId;

    protected EnumerationQuerier()
    {
        _state = -2;
        _threadId = Environment.CurrentManagedThreadId;
    }

    public abstract T Current { get; }

    object? IEnumerator.Current => Current;

    protected abstract EnumerationQuerier<T> Clone();
    public abstract bool MoveNext();

    public IEnumerator<T> GetEnumerator()
    {
        var rtn = _state == -2 && _threadId == Environment.CurrentManagedThreadId ? this : Clone();
        rtn._state = -1;
        return rtn;
    }
    void IEnumerator.Reset() => ThrowHelper.ThrowNotSupport();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Dispose()
    {
        DisposeInternal();
        GC.SuppressFinalize(this);
    }
    protected virtual void DisposeInternal() { }

    private sealed class EmptyQuerier : EnumerationQuerier<T>
    {
        public override T Current => default!;

        public override bool MoveNext() => false;
        protected override EnumerationQuerier<T> Clone() => this;
    }
}
