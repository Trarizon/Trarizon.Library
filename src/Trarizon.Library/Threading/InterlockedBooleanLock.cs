using Trarizon.Library.Threading.Helpers;

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
    private InterlockedBoolean _value;

    public static implicit operator bool(InterlockedBooleanLock interlocked) => interlocked._value;
    public static bool operator true(InterlockedBooleanLock interlocked) => interlocked;
    public static bool operator false(InterlockedBooleanLock interlocked) => interlocked;

    public bool TryEnter()
    {
        if (!LockHelper.InterlockedCompareExchange(ref _value, true, false))
            return true;
        else
            return false;
    }

    public void Enter()
    {
        LockHelper.InterlockedExchange(ref _value, true);
    }

    public void Exit()
    {
        LockHelper.InterlockedExchange(ref _value, false);
    }

    public TryEnteredLock TryEnterLock() 
        => new(this, TryEnter());

    public readonly struct TryEnteredLock(InterlockedBooleanLock @lock, bool entered) : IDisposable
    {
        public bool Entered { get; } = entered;

        public void Dispose()
        {
            if (Entered) {
                @lock.Exit();
            }
        }
    }
}
