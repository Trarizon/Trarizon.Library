namespace Trarizon.Yieliception.Yieliceptors;
public class ValueDeliverer<TYield, TArgs>(
    TYield yieldValue
    ) : IYieliceptor<TArgs>
{
    public TYield YieldValue { get => yieldValue; set => yieldValue = value; }

    public TArgs ReceivedValue { get; internal set; } = default!;

    public ValueDeliverer(out ValueDeliverer<TYield, TArgs> self, TYield yieldValue) : this(yieldValue)
        => self = this;

    // This method only do assignment,
    // If changed, check YieliceptionExtensions.SendAndNext()
    bool IYieliceptor<TArgs>.CanMoveNext(TArgs args)
    {
        ReceivedValue = args;
        return true;
    }

    public ValueDeliverer<TYield, TArgs> WithYield(TYield newYieldValue)
    {
        yieldValue = newYieldValue;
        return this;
    }
}
