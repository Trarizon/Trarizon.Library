namespace Trarizon.Yieliception.Yieliceptors;
public class ValueDeliveryYieliceptor<TArgs>() : IYieliceptor<TArgs>
{
    public TArgs Value { get; private set; } = default!;

    public ValueDeliveryYieliceptor(out ValueDeliveryYieliceptor<TArgs> self) : this()
        => self = this;

    public bool CanMoveNext(TArgs args)
    {
        Value = args;
        return true;
    }
}
