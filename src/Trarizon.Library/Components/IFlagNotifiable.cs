using System.Runtime.CompilerServices;
using Trarizon.Library.Collections;
#if NETSTANDARD
using ListMarshal = Trarizon.Library.Collections.TraCollection;
#else
using ListMarshal = System.Runtime.InteropServices.CollectionsMarshal;
#endif

namespace Trarizon.Library.Components;
public interface IFlagNotifiable<TFlag>
{
    void RegisterNotification(TFlag flag, Action action);
    void UnregisterNotification(TFlag flag, Action action);
}

public interface IFlagNotifiable<TSelf, TFlag> where TSelf : IFlagNotifiable<TSelf, TFlag>
{
    void RegisterNotification(TFlag flag, Action<TSelf> action);
    void UnregisterNotification(TFlag flag, Action<TSelf> action);
}

public static class FlagNotifiable
{
    public static void RegisterGlobalNotification<TFlag>(TFlag flag, Action action)
        => SharedFlagNotifiable<TFlag>.Instance.RegisterNotification(flag, action);

    public static void RegisterGlobalNotificationAndInvoke<TFlag>(TFlag flag, Action action)
        => SharedFlagNotifiable<TFlag>.Instance.RegisterNotificationAndInvoke(flag, action);

    public static void InvokeGlobal<TFlag>(TFlag flag)
        => SharedFlagNotifiable<TFlag>.Instance.NotifyFlag(flag);

    public static void InvokeGlobal<TFlag>(params ReadOnlySpan<TFlag> flags)
        => SharedFlagNotifiable<TFlag>.Instance.NotifyFlag(flags);

    public static void RegisterNotificationAndInvoke<T, TFlag>(this IFlagNotifiable<T, TFlag> self, TFlag flag, Action<T> action, Action<Action>? removeObserverListener = null)
        where T : IFlagNotifiable<T, TFlag>
    {
        self.RegisterNotification(flag, action);
        action.Invoke((T)self);

        if (removeObserverListener is not null)
            removeObserverListener(() => self.UnregisterNotification(flag, action));
    }

    public static void RegisterNotificationAndInvoke<TFlag>(this IFlagNotifiable<TFlag> self, TFlag flag, Action action)
    {
        self.RegisterNotification(flag, action);
        action.Invoke();
    }
}

public abstract class FlagNotifiable<TFlag> : IFlagNotifiable<TFlag>
{
    private readonly List<(TFlag, Action)> _invokers = [];

    public void RegisterNotification(TFlag flag, Action action) => _invokers.Add((flag, action));

    public void UnregisterNotification(TFlag flag, Action action) => _invokers.Remove((flag, action));

    protected void NotifyFlag(TFlag flag)
    {
        if (typeof(TFlag).IsValueType) {
            foreach (var (f, invoker) in ListMarshal.AsSpan(_invokers)) {
                if (EqualityComparer<TFlag>.Default.Equals(flag, f))
                    invoker.Invoke();
            }
        }
        else {
            var comparer = EqualityComparer<TFlag>.Default;
            foreach (var (f, invoker) in ListMarshal.AsSpan(_invokers)) {
                if (comparer.Equals(f, flag))
                    invoker.Invoke();
            }
        }
    }

    protected void NotifyFlag(params ReadOnlySpan<TFlag> flags)
    {
        if (typeof(TFlag).IsValueType) {
            foreach (var (f, invoker) in ListMarshal.AsSpan(_invokers)) {
                if (flags.Contains(TraComparison.CreateEquatable(f)))
                    invoker.Invoke();
            }
        }
        else {
            var comparer = EqualityComparer<TFlag>.Default;
            foreach (var (f, invoker) in ListMarshal.AsSpan(_invokers)) {
                if (flags.Contains(TraComparison.CreateEquatable(f, comparer)))
                    invoker.Invoke();
            }
        }
    }
}

public abstract class FlagNotifiable<TSelf, TFlag> : IFlagNotifiable<TSelf, TFlag> where TSelf : FlagNotifiable<TSelf, TFlag>
{
    private readonly List<(TFlag, Action<TSelf>)> _invokers = [];

    public void RegisterNotification(TFlag flag, Action<TSelf> action) => _invokers.Add((flag, action));
    public void UnregisterNotification(TFlag flag, Action<TSelf> action) => _invokers.Remove((flag, action));

    protected void NotifyFlag(TFlag flag)
    {
        if (_invokers is null)
            return;

        if (typeof(TFlag).IsValueType) {
            foreach (var (f, invoker) in ListMarshal.AsSpan(_invokers)) {
                if (EqualityComparer<TFlag>.Default.Equals(flag, f))
                    invoker.Invoke(Unsafe.As<TSelf>(this));
            }
        }
        else {
            var comparer = EqualityComparer<TFlag>.Default;
            foreach (var (f, invoker) in ListMarshal.AsSpan(_invokers)) {
                if (comparer.Equals(f, flag))
                    invoker.Invoke(Unsafe.As<TSelf>(this));
            }
        }
    }

    protected void NotifyFlag(params ReadOnlySpan<TFlag> flags)
    {
        if (typeof(TFlag).IsValueType) {
            foreach (var (f, invoker) in ListMarshal.AsSpan(_invokers)) {
                if (flags.Contains(TraComparison.CreateEquatable(f)))
                    invoker.Invoke(Unsafe.As<TSelf>(this));
            }
        }
        else {
            var comparer = EqualityComparer<TFlag>.Default;
            foreach (var (f, invoker) in ListMarshal.AsSpan(_invokers)) {
                if (flags.Contains(TraComparison.CreateEquatable(f, comparer)))
                    invoker.Invoke(Unsafe.As<TSelf>(this));
            }
        }
    }
}

internal sealed class SharedFlagNotifiable<TFlag> : IFlagNotifiable<TFlag>
{
    public static SharedFlagNotifiable<TFlag> Instance { get; } = new();

    private readonly List<(TFlag, Action)> _invokers = [];
#if NET9_0_OR_GREATER
    private readonly Lock _lock = new();
#else
    private readonly object _lock = new();
#endif

    private SharedFlagNotifiable() { }

    public void RegisterNotification(TFlag flag, Action action)
    {
        lock (_lock)
            _invokers.Add((flag, action));
    }

    public void UnregisterNotification(TFlag flag, Action action)
    {
        lock (_lock)
            _invokers.Remove((flag, action));
    }

    public void NotifyFlag(TFlag flag)
    {
        if (typeof(TFlag).IsValueType) {
            lock (_lock) {
                foreach (var (f, invoker) in ListMarshal.AsSpan(_invokers)) {
                    if (EqualityComparer<TFlag>.Default.Equals(flag, f))
                        invoker.Invoke();
                }
            }
        }
        else {
            var comparer = EqualityComparer<TFlag>.Default;
            lock (_lock) {
                foreach (var (f, invoker) in ListMarshal.AsSpan(_invokers)) {
                    if (comparer.Equals(f, flag))
                        invoker.Invoke();
                }
            }
        }
    }

    public void NotifyFlag(params ReadOnlySpan<TFlag> flags)
    {
        if (_invokers is null)
            return;

        if (typeof(TFlag).IsValueType) {
            foreach (var (f, invoker) in ListMarshal.AsSpan(_invokers)) {
                if (flags.Contains(TraComparison.CreateEquatable(f)))
                    invoker.Invoke();
            }
        }
        else {
            var comparer = EqualityComparer<TFlag>.Default;
            foreach (var (f, invoker) in ListMarshal.AsSpan(_invokers)) {
                if (flags.Contains(TraComparison.CreateEquatable(f, comparer)))
                    invoker.Invoke();
            }
        }
    }
}

public struct FlagNotifier<TFlag>
{
    public List<(TFlag, Action)>? _invokers;

    public void AddListener(TFlag flag, Action action)
    {
        (_invokers ??= []).Add((flag, action));
    }

    public readonly void RemoveListener(TFlag flag, Action action)
        => _invokers?.Remove((flag, action));

    public readonly void Invoke(TFlag flag)
    {
        if (_invokers is null)
            return;

        if (typeof(TFlag).IsValueType) {
            foreach (var (f, invoker) in ListMarshal.AsSpan(_invokers)) {
                if (EqualityComparer<TFlag>.Default.Equals(flag, f))
                    invoker.Invoke();
            }
        }
        else {
            var comparer = EqualityComparer<TFlag>.Default;
            foreach (var (f, invoker) in ListMarshal.AsSpan(_invokers)) {
                if (comparer.Equals(f, flag))
                    invoker.Invoke();
            }
        }
    }

    public readonly void Invoke(params ReadOnlySpan<TFlag> flags)
    {
        if (_invokers is null)
            return;

        if (typeof(TFlag).IsValueType) {
            foreach (var (f, invoker) in ListMarshal.AsSpan(_invokers)) {
                if (flags.Contains(TraComparison.CreateEquatable(f)))
                    invoker.Invoke();
            }
        }
        else {
            var comparer = EqualityComparer<TFlag>.Default;
            foreach (var (f, invoker) in ListMarshal.AsSpan(_invokers)) {
                if (flags.Contains(TraComparison.CreateEquatable(f, comparer)))
                    invoker.Invoke();
            }
        }
    }
}

public struct FlagNotifier<TSender, TFlag>
{
    public List<(TFlag, Action<TSender>)>? _invokers;

    public void AddListener(TFlag flag, Action<TSender> action)
    {
        (_invokers ??= []).Add((flag, action));
    }

    public readonly void RemoveListener(TFlag flag, Action<TSender> action)
    {
        _invokers?.Remove((flag, action));
    }

    public readonly void Invoke(TSender sender, TFlag flag)
    {
        if (_invokers is null)
            return;

        if (typeof(TFlag).IsValueType) {
            foreach (var (f, invoker) in _invokers) {
                if (EqualityComparer<TFlag>.Default.Equals(flag, f))
                    invoker.Invoke(sender);
            }
        }
        else {
            var comparer = EqualityComparer<TFlag>.Default;
            foreach (var (f, invoker) in _invokers) {
                if (comparer.Equals(f, flag))
                    invoker.Invoke(sender);
            }
        }
    }

    public readonly void Invoke(TSender sender, params ReadOnlySpan<TFlag> flags)
    {
        if (_invokers is null)
            return;

        if (typeof(TFlag).IsValueType) {
            foreach (var (f, invoker) in _invokers) {
                if (flags.Contains(TraComparison.CreateEquatable(f)))
                    invoker.Invoke(sender);
            }
        }
        else {
            var comparer = EqualityComparer<TFlag>.Default;
            foreach (var (f, invoker) in _invokers) {
                if (flags.Contains(TraComparison.CreateEquatable(f, comparer)))
                    invoker.Invoke(sender);
            }
        }
    }
}
