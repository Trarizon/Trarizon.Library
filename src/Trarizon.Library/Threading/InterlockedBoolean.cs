using Trarizon.Library.CodeAnalysis.MemberAccess;
using Trarizon.Library.Helpers;

namespace Trarizon.Library.Threading;
public struct InterlockedBoolean
{
    internal const int TrueValue = 1;
    internal const int FalseValue = 0;

    [FriendAccess(typeof(LockHelper))]
    internal int _value;

    public readonly bool Value => _value != 0;

    public static implicit operator bool(InterlockedBoolean interlocked) => interlocked.Value;
    public static bool operator true(InterlockedBoolean interlocked) => interlocked;
    public static bool operator false(InterlockedBoolean interlocked) => interlocked;

    public bool TryEnter()
    {
        if (0 == Interlocked.CompareExchange(ref _value, 1, 0))
            return true;
        else
            return false;
    }

    public void Enter()
    {
        Interlocked.Exchange(ref _value, 1);
    }

    public void Exit()
    {
        Interlocked.Exchange(ref _value, 0);
    }
}
