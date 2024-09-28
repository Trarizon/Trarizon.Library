namespace Trarizon.Library.Threading;
/// <remarks>
/// Use <see cref="TryEnterLock"/> can return a disposable wrapper
/// of <see cref="InterlockedBooleanLock"/>, which will auto exit on 
/// <see cref="IDisposable.Dispose"/>
/// <code>
/// var _lock = new InterlockedBooleanLock();
/// 
/// using var l = _lock.TryEnterLock();
/// if (l.Entered) { }
/// else { }
/// </code>
/// </remarks>
public sealed class InterlockedBooleanLock
{
    private volatile int _flag;

    public TryEnteredScope TryEnter()
    {
        return new TryEnteredScope(this, 
            Interlocked.CompareExchange(ref _flag, 1, 0) != 0);
    }

    public void Enter()
    {
        while (true) {
            if (Interlocked.CompareExchange(ref _flag, 1, 0) == 0)
                return;

            Thread.SpinWait(1);
        }
    }

    public Scope EnterScope()
    {
        while (true) {
            if (Interlocked.CompareExchange(ref _flag, 1, 0) == 0)
                return new Scope(this);

            Thread.SpinWait(1);
        }
    }

    public void Exit()
    {
        Interlocked.Exchange(ref _flag, 0);
    }

    public readonly ref struct Scope
    {
        private readonly InterlockedBooleanLock _lock;

        internal Scope(InterlockedBooleanLock @lock) => _lock = @lock;

        public void Dispose()
        {
            _lock?.Exit();
        }
    }

    public readonly ref struct TryEnteredScope
    {
        private readonly InterlockedBooleanLock _lock;

        internal TryEnteredScope(InterlockedBooleanLock @lock, bool entered)
        {
            _lock = @lock;
            Entered = entered;
        }

        public bool Entered { get; }

        public void Dispose()
        {
            if (Entered) {
                _lock?.Exit();
            }
        }
    }
}
