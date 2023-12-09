using Trarizon.Library.Wrappers;
using Trarizon.Yieliception.Components;

namespace Trarizon.Yieliception.Yieliceptors;
public class ValidationYieliceptor<TArgs, TResult>(
    Func<TArgs, Optional<TResult>> validator,
    Action<ValidationYieliceptor<TArgs, TResult>, TArgs>? onRejected = null
    ) : IYieliceptor<TArgs>, ITimerYieliceptor
{
    private TResult? _result;
    public TResult Result => _result!;

    #region Timer properties
    public int TimeLimitMillisecond { get; set; } = Timeout.Infinite;
    public bool ResetTimerOnRejected { get; set; }
    public bool IsTimeout { get; private set; }
    public void SetTimeout() => IsTimeout = true;
    #endregion

    public ValidationYieliceptor(out ValidationYieliceptor<TArgs, TResult> self,
        Func<TArgs, Optional<TResult>> validator,
        Action<ValidationYieliceptor<TArgs, TResult>, TArgs>? onRejected = null) : this(validator, onRejected)
        => self = this;

    public bool CanMoveNext(TArgs args)
    {
        if (validator(args) is (true, var result)) {
            IsTimeout = false;
            _result = result;
            return true;
        }
        else {
            onRejected?.Invoke(this, args);
            return false;
        }
    }

    public ValidationYieliceptor<TArgs, TResult> WithValidator(Func<TArgs, Optional<TResult>> newValidator)
    {
        validator = newValidator;
        return this;
    }

    public ValidationYieliceptor<TArgs, TResult> WithRejectionHandler(Action<ValidationYieliceptor<TArgs, TResult>, TArgs>? newOnReject)
    {
        onRejected = newOnReject;
        return this;
    }
}

public class ValidationYieliceptor<TArgs>(
    Func<TArgs, bool> validator,
    Action<ValidationYieliceptor<TArgs, TArgs>, TArgs>? onRejected = null
    ) : ValidationYieliceptor<TArgs, TArgs>(validator.Currying, onRejected)
{
    public ValidationYieliceptor(out ValidationYieliceptor<TArgs> self,
        Func<TArgs, bool> validator,
        Action<ValidationYieliceptor<TArgs, TArgs>, TArgs>? onRejected = null) : this(validator, onRejected)
        => self = this;

    public ValidationYieliceptor<TArgs> WithValidator(Func<TArgs, bool> newValidator)
    {
        WithValidator(newValidator.Currying);
        return this;
    }
}

file static class FuncCurrying
{
    public static Optional<TArgs> Currying<TArgs>(this Func<TArgs, bool> validator, TArgs args)
        => validator(args) ? args : default(Optional<TArgs>);
}