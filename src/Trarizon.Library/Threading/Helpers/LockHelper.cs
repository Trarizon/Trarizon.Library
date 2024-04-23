namespace Trarizon.Library.Threading.Helpers;
public static class LockHelper
{
    /// <summary>
    /// </summary>
    /// <param name="value"></param>
    /// <param name="comparand"></param>
    /// <returns>The original value</returns>
    public static bool InterlockedCompareExchange(ref InterlockedBoolean refValue, bool value, bool comparand)
    {
        return InterlockedBoolean.TrueValue == Interlocked.CompareExchange(ref refValue._value,
            value ? InterlockedBoolean.TrueValue : InterlockedBoolean.FalseValue,
            comparand ? InterlockedBoolean.TrueValue : InterlockedBoolean.FalseValue);
    }

    /// <summary>
    /// </summary>
    /// <param name="value"></param>
    /// <returns>The oringal value</returns>
    public static bool InterlockedExchange(ref InterlockedBoolean refValue, bool value)
    {
        return InterlockedBoolean.TrueValue == Interlocked.Exchange(ref refValue._value,
            value ? InterlockedBoolean.TrueValue : InterlockedBoolean.FalseValue);
    }

    public static void InterlockToggle(ref InterlockedBoolean interlockedBoolean)
    {
        InterlockedExchange(ref interlockedBoolean, interlockedBoolean);
    }
}
