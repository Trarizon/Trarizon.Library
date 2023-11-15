using System.Diagnostics;
using Trarizon.Library.Collections.Extensions;
using Trarizon.Yieliception.Components;
using Trarizon.Yieliception.Yieliceptors;

namespace Trarizon.Yieliception;
public class YieliceptableIterator<T>(
    IEnumerator<IYieliceptor<T>?> enumerator,
    bool threadSafe,
    IYieliceptionComponent[] components) : IYieliceptableIterator<T>, IDisposable
{
    private readonly object? _lockobj = threadSafe ? null : new object();
    private IEnumerator<IYieliceptor<T>?>? _enumerator = enumerator;

    public event Action<YieliceptableIterator<T>>? End;

    public bool IsEnded => _enumerator is null;
    public IYieliceptor<T>? CurrentYieliceptor => _enumerator?.Current;
    public IYieliceptionComponent[] Components => components;

    public YieliceptionResult Next(T args) => ThreadSafeNext(true, args, false);

    public YieliceptionResult ForceNext(bool returnIfOccupied) => ThreadSafeNext(false, default, returnIfOccupied);

    private YieliceptionResult ThreadSafeNext(bool check, T? args, bool returnIfOccupied)
    {
        if (IsEnded)
            return YieliceptionResult.Ended;
        if (_lockobj is null)
            return Next(check, args!);

        // Lock
        if (returnIfOccupied) {
            if (!Monitor.TryEnter(_lockobj))
                return YieliceptionResult.Occupied;
        }
        else {
            Monitor.Enter(_lockobj);
        }

        try {
            return Next(check, args!);
        } finally {
            Monitor.Exit(_lockobj);
        }

        // Impl

        YieliceptionResult Next(bool check, T args)
        {
            Debug.Assert(_enumerator is not null);

            if (check && _enumerator.Current?.CanMoveNext(args) == false) {
                foreach (var c in components)
                    c.OnNext(this, false);
                return YieliceptionResult.Rejected;
            }
            else if (_enumerator.MoveNext()) {
                foreach (var c in components)
                    c.OnNext(this, true);
                return YieliceptionResult.Moved;
            }
            else {
                End?.Invoke(this);
                Dispose();
                return YieliceptionResult.Ended;
            }
        }
    }

    public void Dispose()
    {
        foreach (var c in components) {
            c.Dispose();
        }
        _enumerator?.Dispose();
        _enumerator = null;
        GC.SuppressFinalize(this);
    }
}
