using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Trarizon.Library.Components;
[Experimental("TRAEVB")]
public static class EventBus
{
    public static bool UseDictionaryByDefault { get; set; }

    public static void AddListener<TEventArgs>(Action<TEventArgs> listener)
    {
        if (UseDictionaryByDefault)
            EventProvider<TEventArgs>.Event += listener;
        else
            EventDictionaryProvider.Add(listener);
    }

    public static void RemoveListener<TEventArgs>(Action<TEventArgs> listener)
    {
        if (UseDictionaryByDefault)
            EventProvider<TEventArgs>.Event -= listener;
        else
            EventDictionaryProvider.Remove(listener);
    }

    public static void Invoke<TEventArgs>(TEventArgs args)
    {
        if (UseDictionaryByDefault)
            EventProvider<TEventArgs>.Invoke(args);
        else
            EventDictionaryProvider.Invoke(args);
    }

    private static class EventProvider<TEventArgs>
    {
        public static event Action<TEventArgs>? Event;

        internal static void Invoke(TEventArgs args) => Event?.Invoke(args);
    }

    private static class EventDictionaryProvider
    {
        private static readonly Dictionary<Type, MulticastDelegate?> _dict = new();

#if !NET8_0_OR_GREATER
        private static bool TryGet<TEventArgs>(out Action<TEventArgs>? ev)
        {
            if (_dict.TryGetValue(typeof(TEventArgs), out var dele)) {
                Debug.Assert(dele is Action<TEventArgs>);
                ev = Unsafe.As<Action<TEventArgs>?>(dele);
                return true;
            }
            ev = null;
            return false;
        }
#endif

        public static void Add<TEventArgs>(Action<TEventArgs> action)
        {
#if NET8_0_OR_GREATER
            ref var item = ref CollectionsMarshal.GetValueRefOrAddDefault(_dict, typeof(TEventArgs), out var exists);
            ref var ev = ref Unsafe.As<MulticastDelegate?, Action<TEventArgs>?>(ref item);
            ev += action;
#else
            if (TryGet<TEventArgs>(out var ev)) {
                ev += action;
                _dict[typeof(TEventArgs)] = ev;
            }
            else {
                _dict.Add(typeof(TEventArgs), action);
            }
#endif
        }

        public static void Remove<TEventArgs>(Action<TEventArgs> action)
        {
#if NET8_0_OR_GREATER
            ref var item = ref CollectionsMarshal.GetValueRefOrNullRef(_dict, typeof(TEventArgs));
            if (Unsafe.IsNullRef(in item))
                return;
            ref var act = ref Unsafe.As<MulticastDelegate?, Action<TEventArgs>?>(ref item);
            act -= action;
#else
            if (TryGet<TEventArgs>(out var ev)) {
                ev -= action;
                _dict[typeof(TEventArgs)] = ev;
            }
#endif
        }

        public static void Invoke<TEventArgs>(TEventArgs args)
        {
            if (_dict.TryGetValue(typeof(TEventArgs), out var dele)) {
                var action = Unsafe.As<Action<TEventArgs>>(dele);
                action?.Invoke(args);
            }
        }
    }
}
