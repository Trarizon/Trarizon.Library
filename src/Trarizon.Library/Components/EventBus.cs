using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Trarizon.Library.Components;
[Experimental("TRALIB")]
public class DictionaryEventBus
{
    private readonly Dictionary<Type, MulticastDelegate> _handlers;

    public DictionaryEventBus()
    {
        _handlers = [];
    }

#if !NET8_0_OR_GREATER
    private bool TryGetHandler<TEventArgs>([MaybeNullWhen(false)] out Action<TEventArgs> ev)
    {
        if (_handlers.TryGetValue(typeof(TEventArgs), out var handler)) {
            Debug.Assert(handler is Action<TEventArgs>);
            ev = Unsafe.As<Action<TEventArgs>>(handler);
            return true;
        }
        ev = null;
        return false;
    }
#endif

    public void AddListener<TEventArgs>(Action<TEventArgs> listener)
    {
#if NET8_0_OR_GREATER
        ref var item = ref CollectionsMarshal.GetValueRefOrAddDefault(_handlers, typeof(TEventArgs), out var exists);
        ref var ev = ref Unsafe.As<MulticastDelegate?, Action<TEventArgs>?>(ref item);
        ev += listener;
#else
        if (TryGetHandler<TEventArgs>(out var handler)) {
            handler += listener;
            _handlers[typeof(TEventArgs)] = handler;
        }
        else {
            _handlers.Add(typeof(TEventArgs), listener);
        }
#endif
    }

    public void RemoveListener<TEventArgs>(Action<TEventArgs> listener)
    {
#if NET8_0_OR_GREATER
        ref var item = ref CollectionsMarshal.GetValueRefOrNullRef(_handlers, typeof(TEventArgs));
        if (Unsafe.IsNullRef(in item))
            return;
        ref var handler = ref Unsafe.As<MulticastDelegate?, Action<TEventArgs>?>(ref item);
        handler -= listener;
        if (handler is null) {
            _handlers.Remove(typeof(TEventArgs));
        }
#else
        if (TryGetHandler<TEventArgs>(out var handler)) {
            handler -= listener;
            if (handler is null)
                _handlers.Remove(typeof(TEventArgs));
            else
                _handlers[typeof(TEventArgs)] = handler;
        }
#endif
    }

    public void Invoke<TEventArgs>(in TEventArgs args)
    {
#if NET8_0_OR_GREATER
        if (_handlers.TryGetValue(typeof(TEventArgs), out var handler)) {
            Unsafe.As<Action<TEventArgs>?>(handler)?.Invoke(args);
        }
#else
        if (TryGetHandler<TEventArgs>(out var handler)) {
            handler?.Invoke(args);
        }
#endif
    }
}

[Experimental("TRALIB")]
public static class EventBus
{
    public static void AddListener<TEventArgs>(Action<TEventArgs> listener)
    {
        EventProvider<TEventArgs>.Event += listener;
    }

    public static void RemoveListener<TEventArgs>(Action<TEventArgs> listener)
    {
        EventProvider<TEventArgs>.Event -= listener;
    }

    public static void Invoke<TEventArgs>(TEventArgs args)
    {
        EventProvider<TEventArgs>.Invoke(args);
    }

    private static class EventProvider<TEventArgs>
    {
        public static event Action<TEventArgs>? Event;

        internal static void Invoke(TEventArgs args) => Event?.Invoke(args);
    }
}
