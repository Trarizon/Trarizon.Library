using Trarizon.Library.Threading;

namespace Trarizon.Library.Helpers;
public static class LockHelper
{
    public static bool InterlockedCompareExchange(ref InterlockedBoolean interlockedBoolean, bool value, bool comparand)
    {
        return InterlockedBoolean.TrueValue == Interlocked.CompareExchange(ref interlockedBoolean._value,
            value ? InterlockedBoolean.TrueValue : InterlockedBoolean.FalseValue,
            comparand ? InterlockedBoolean.TrueValue : InterlockedBoolean.FalseValue);
    }

    public static bool InterlockedExchange(ref InterlockedBoolean interlockedBoolean,bool value)
    {
        return InterlockedBoolean.TrueValue == Interlocked.Exchange(ref interlockedBoolean._value,
            value ? InterlockedBoolean.TrueValue : InterlockedBoolean.FalseValue);
    }
}
