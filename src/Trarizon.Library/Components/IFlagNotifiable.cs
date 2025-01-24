using CommunityToolkit.HighPerformance;
using System.Runtime.CompilerServices;
using Trarizon.Library.CodeGeneration;
using Trarizon.Library.Collections;

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

    public static void RegisterNotificationAndInvoke<TSelf, TFlag>(this TSelf self, TFlag flag, Action<TSelf> action, Action<Action> removerOberserListener)
        where TSelf : IFlagNotifiable<TSelf, TFlag>
    {
        self.RegisterNotification(flag, action);
        action.Invoke(self);
        removerOberserListener(() => self.UnregisterNotification(flag, action));
    }

    public static void RegisterNotificationAndInvoke<TSelf, TFlag>(this TSelf self, TFlag flag, Action<TSelf> action)
        where TSelf : IFlagNotifiable<TSelf, TFlag>
    {
        self.RegisterNotification(flag, action);
        action.Invoke(self);
    }

    public static void RegisterNotificationAndInvoke<TSelf, TFlag>(this TSelf self, TFlag flag, Action action)
        where TSelf : IFlagNotifiable<TFlag>
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
            foreach (var (f, invoker) in _invokers.AsSpan()) {
                if (EqualityComparer<TFlag>.Default.Equals(flag, f))
                    invoker.Invoke();
            }
        }
        else {
            var comparer = EqualityComparer<TFlag>.Default;
            foreach (var (f, invoker) in _invokers.AsSpan()) {
                if (comparer.Equals(f, flag))
                    invoker.Invoke();
            }
        }
    }

    protected void NotifyFlag(params ReadOnlySpan<TFlag> flags)
    {
        if (typeof(TFlag).IsValueType) {
            foreach (var (f, invoker) in _invokers.AsSpan()) {
                if (flags.ContainsByComparer(f))
                    invoker.Invoke();
            }
        }
        else {
            var comparer = EqualityComparer<TFlag>.Default;
            foreach (var (f, invoker) in _invokers.AsSpan()) {
                if (flags.ContainsByComparer(f, comparer))
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
            foreach (var (f, invoker) in _invokers.AsSpan()) {
                if (EqualityComparer<TFlag>.Default.Equals(flag, f))
                    invoker.Invoke(Unsafe.As<TSelf>(this));
            }
        }
        else {
            var comparer = EqualityComparer<TFlag>.Default;
            foreach (var (f, invoker) in _invokers.AsSpan()) {
                if (comparer.Equals(f, flag))
                    invoker.Invoke(Unsafe.As<TSelf>(this));
            }
        }
    }

    protected void NotifyFlag(params ReadOnlySpan<TFlag> flags)
    {
        if (typeof(TFlag).IsValueType) {
            foreach (var (f, invoker) in _invokers.AsSpan()) {
                if (flags.ContainsByComparer(f))
                    invoker.Invoke(Unsafe.As<TSelf>(this));
            }
        }
        else {
            var comparer = EqualityComparer<TFlag>.Default;
            foreach (var (f, invoker) in _invokers.AsSpan()) {
                if (flags.ContainsByComparer(f, comparer))
                    invoker.Invoke(Unsafe.As<TSelf>(this));
            }
        }
    }
}

[Singleton]
internal sealed partial class SharedFlagNotifiable<TFlag> : IFlagNotifiable<TFlag>
{
    private readonly List<(TFlag, Action)> _invokers = [];
#if NET9_0_OR_GREATER
    private readonly Lock _lock = new();
#else
    private readonly object _lock = new();
#endif

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
                foreach (var (f, invoker) in _invokers.AsSpan()) {
                    if (EqualityComparer<TFlag>.Default.Equals(flag, f))
                        invoker.Invoke();
                }
            }
        }
        else {
            var comparer = EqualityComparer<TFlag>.Default;
            lock (_lock) {
                foreach (var (f, invoker) in _invokers.AsSpan()) {
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
            foreach (var (f, invoker) in _invokers.AsSpan()) {
                if (flags.ContainsByComparer(f))
                    invoker.Invoke();
            }
        }
        else {
            var comparer = EqualityComparer<TFlag>.Default;
            foreach (var (f, invoker) in _invokers.AsSpan()) {
                if (flags.ContainsByComparer(f, comparer))
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
            foreach (var (f, invoker) in _invokers.AsSpan()) {
                if (EqualityComparer<TFlag>.Default.Equals(flag, f))
                    invoker.Invoke();
            }
        }
        else {
            var comparer = EqualityComparer<TFlag>.Default;
            foreach (var (f, invoker) in _invokers.AsSpan()) {
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
            foreach (var (f, invoker) in _invokers.AsSpan()) {
                if (flags.ContainsByComparer(f))
                    invoker.Invoke();
            }
        }
        else {
            var comparer = EqualityComparer<TFlag>.Default;
            foreach (var (f, invoker) in _invokers.AsSpan()) {
                if (flags.ContainsByComparer(f, comparer))
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
                if (flags.ContainsByComparer(f))
                    invoker.Invoke(sender);
            }
        }
        else {
            var comparer = EqualityComparer<TFlag>.Default;
            foreach (var (f, invoker) in _invokers) {
                if (flags.ContainsByComparer(f, comparer))
                    invoker.Invoke(sender);
            }
        }
    }
}
