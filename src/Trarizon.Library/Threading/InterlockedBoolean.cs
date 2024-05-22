using Trarizon.Library.CodeAnalysis.MemberAccess;
using Trarizon.Library.Threading.Helpers;

namespace Trarizon.Library.Threading;
public struct InterlockedBoolean(bool value)
{
    internal const int TrueValue = 1;
    internal const int FalseValue = 0;

    [FriendAccess(typeof(LockHelper), typeof(InterlockedBooleanLock))]
    internal int _value = value ? TrueValue : FalseValue;

    public readonly bool Value => _value != 0;

    public static implicit operator InterlockedBoolean(bool boolean) => new(boolean);
    public static implicit operator bool(InterlockedBoolean interlocked) => interlocked.Value;
    public static bool operator true(InterlockedBoolean interlocked) => interlocked;
    public static bool operator false(InterlockedBoolean interlocked) => !interlocked;
    public static InterlockedBoolean operator !(InterlockedBoolean interlocked) => new(!interlocked.Value);
}
