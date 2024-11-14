using Trarizon.Library.Collections;

namespace Trarizon.Library.Components;
public interface IFlagNotifiable<TSelf, TFlag> where TSelf : IFlagNotifiable<TSelf, TFlag>
{
    void RegisterNotification(TFlag flag, Action<TSelf> action);
}

public static class IFlagNotifiableExt
{
    public static void RegisterNotificationAndInvoke<TSelf, TFlag>(this TSelf self, TFlag flag, Action<TSelf> action)
        where TSelf : IFlagNotifiable<TSelf, TFlag>
    {
        self.RegisterNotification(flag, action);
        action.Invoke(self);
    }
}

public struct FlagNotifier<TSender, TFlag>
{
    public List<(TFlag, Action<TSender>)>? _invokers;

    public void AddListener(TFlag flag, Action<TSender> action)
    {
        (_invokers ??= []).Add((flag, action));
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

        foreach (var (f, invoker) in _invokers) {
            if (flags.ContainsByComparer(f))
                invoker.Invoke(sender);
        }
    }
}