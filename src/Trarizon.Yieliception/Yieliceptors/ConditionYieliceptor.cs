using Trarizon.Yieliception.Components;

namespace Trarizon.Yieliception.Yieliceptors;
public class ConditionYieliceptor(
    Func<bool> validator,
    Action<ConditionYieliceptor>? onRejected = null
    ) : IYieliceptor, ITimerYieliceptor
{
    #region Timer properties
    public int TimeLimitMillisecond { get; set; } = Timeout.Infinite;
    public bool ResetTimerOnRejected { get; set; }
    public void SetTimeout() { }
    #endregion

    public ConditionYieliceptor(out ConditionYieliceptor self, 
        Func<bool> validator, Action<ConditionYieliceptor>? onRejected = null) :
        this(validator, onRejected)
        => self = this;

    public bool CanMoveNext(object args)
    {
        if (validator()) {
            return true;
        }
        else {
            onRejected?.Invoke(this);
            return false;
        }
    }

    public static implicit operator ConditionYieliceptor(Func<bool> validator)
        => new(validator);

    public ConditionYieliceptor WithValidator(Func<bool> newValidator)
    {
        validator = newValidator;
        return this;
    }

    public ConditionYieliceptor WithRejectionHandler(Action<ConditionYieliceptor>? newOnReject)
    {
        onRejected = newOnReject;
        return this;
    }
}
