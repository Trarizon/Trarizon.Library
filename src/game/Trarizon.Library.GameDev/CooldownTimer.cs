namespace Trarizon.Library.GameDev;
public struct CooldownTimer
{
    private readonly float _maxTime;
    private float _t;

    public readonly float MaxTime => _maxTime;

    public readonly float ElapsedTime => _t;

    public CooldownTimer(float maxTime)
    {
        _maxTime = maxTime;
        _t = 0;
    }

    public void Reset() => _t = 0;

    public bool Elapse(float deltaTime)
    {
        _t += deltaTime;
        if (_t >= _maxTime) {
            _t -= _maxTime;
            return true;
        }
        else {
            return false;
        }
    }

    public static bool Elapse(ref float timer, float maxTime, float deltaTime)
    {
        timer += deltaTime;
        if (timer >= maxTime) {
            timer -= maxTime;
            return true;
        }
        else {
            return false;
        }
    }
}
