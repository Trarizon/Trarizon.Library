using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Yieliception.Components;
public interface ITimerYieliceptor
{
    int TimeLimitMillisecond { get; }
    bool ResetTimerOnRejected { get; }
    void SetTimeout();
}

public sealed class YieliceptionTimer : IYieliceptionComponent, IAsyncYieliceptionComponent
{
    private Timer? _timer;

    public void OnNext<T>(AsyncYieliceptableIterator<T> iterator, bool moved)
        => OnNextInternal(iterator, moved);

    public void OnNext<T>(YieliceptableIterator<T> iterator, bool moved)
        => OnNextInternal(iterator, moved);

    private void OnNextInternal<T>(IYieliceptableIterator<T> iterator, bool moved)
    {
        if (iterator.CurrentYieliceptor is ITimerYieliceptor yieliceptor) {
            // If moved, reset timer,
            // If rejected, reset when ResetTimerOnRejected is true
            if (moved || yieliceptor.ResetTimerOnRejected) {
                EnsureTimer(iterator);
                _timer.Change(yieliceptor.TimeLimitMillisecond, Timeout.Infinite);
            }
        }
        else
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
    }

    [MemberNotNull(nameof(_timer))]
    private void EnsureTimer<T>(IYieliceptableIterator<T> iterator)
    {
        _timer ??= new Timer(static obj => // This lambda will be called when Reset()
        {
            // (timer, iterator)
            var tuple = (Tuple<YieliceptionTimer, IYieliceptableIterator<T>>)obj!;

            // Pause to avoid some conflict
            tuple.Item1._timer!.Change(Timeout.Infinite, Timeout.Infinite);

            Debug.Assert(tuple.Item2 is ITimerYieliceptor);
            var yieliceptor = (ITimerYieliceptor)tuple.Item2.CurrentYieliceptor!;
            yieliceptor.SetTimeout();
            tuple.Item2.ForceNext(true);
        },
        Tuple.Create(this, iterator), Timeout.Infinite, Timeout.Infinite);
    }

    public void Dispose() => _timer?.Dispose();
}
