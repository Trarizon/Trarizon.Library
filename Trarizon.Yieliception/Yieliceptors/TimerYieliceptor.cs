using Trarizon.Yieliception.Components;

namespace Trarizon.Yieliception.Yieliceptors;
public class TimerYieliceptor() : IYieliceptor, ITimerYieliceptor
{
    public TimerYieliceptor(out TimerYieliceptor self) : this()
        => self = this;

    public int TimeLimitMillisecond { get; set; } = Timeout.Infinite;
    public bool ResetTimerOnRejected { get; set; }
    public bool IsTimeout { get; private set; }
    public void SetTimeout() => IsTimeout = true;

    public bool CanMoveNext(object args)
    {
        IsTimeout = false;
        return true;
    }
}
