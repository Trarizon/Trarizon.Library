using CommunityToolkit.Diagnostics;
using System.Collections;

namespace Trarizon.Library.Collections;
partial class TraEnumerable
{
    private abstract class IteratorBase<T> : IEnumerable<T>, IEnumerator<T>
    {
        protected const int MinPreservedState = -2;
        protected const int InitState = -1;

        protected int _state = -2;
        private readonly int _threadId = Environment.CurrentManagedThreadId;

        public abstract T Current { get; }
        protected abstract IteratorBase<T> Clone();
        public abstract bool MoveNext();
        protected virtual void DisposeInternal() { }
        public virtual void Reset() => ThrowHelper.ThrowNotSupportedException();

        public IEnumerator<T> GetEnumerator()
        {
            var rtn = _state == -2 && _threadId == Environment.CurrentManagedThreadId ? this : Clone();
            rtn._state = -1;
            return rtn;
        }

        public void Dispose()
        {
            DisposeInternal();
            GC.SuppressFinalize(this);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

#nullable disable
        object IEnumerator.Current => Current;
#nullable restore
    }
}
