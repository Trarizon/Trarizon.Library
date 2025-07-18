﻿namespace Trarizon.Library.Game;
public struct ManualResetCooldownTimer
{
    private readonly float _maxTime;
    private float _t;

    public readonly float MaxTime => _maxTime;

    public readonly float ElapsedTime => _t >= _maxTime ? _maxTime : _t;

    public readonly float ElapsedPercentage => _t >= _maxTime ? 1f : _t / _maxTime;

    public readonly bool IsCompleted => _t >= _maxTime;

    public ManualResetCooldownTimer(float maxTime)
    {
        _maxTime = maxTime;
        _t = 0;
    }

    public void Reset() => _t = 0;

    public bool Elapse(float deltaTime) => Elapse(ref _t, _maxTime, deltaTime, false);

    public bool ElapseOrCompleted(float deltaTime) => Elapse(ref _t, _maxTime, deltaTime, true);

    public bool ElapseAutoReset(float deltaTime) => CooldownTimer.Elapse(ref _t, _maxTime, deltaTime);

    public static bool Elapse(ref float timer, float maxTime, float deltaTime, bool returnTrueOnAlreadyCompleted = false)
    {
        if (timer >= maxTime)
            return returnTrueOnAlreadyCompleted;

        timer += deltaTime;
        if (timer >= maxTime) {
            return true;
        }
        else {
            return false;
        }
    }
}
