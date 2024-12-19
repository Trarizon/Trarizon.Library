using CommunityToolkit.HighPerformance;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
#if NETSTANDARD2_0
using Unsafe = Trarizon.Library.Netstd.NetstdFix_Unsafe;
#endif

namespace Trarizon.Library.Collections;
partial class TraList
{
    public static Lookup<T> GetLookup<T>(this List<T> list, IEqualityComparer<T>? comparer = null)
        => new(list, comparer ?? EqualityComparer<T>.Default);
    /*
    public static KeyedLookup<T, TKey> GetKeyedLookup<T, TKey>(this List<T> list, IKeyedEqualityComparer<T, TKey> comparer)
        => new(list, comparer);

    public static KeyedLookup<(TKey, TValue), TKey> GetKeyedLookup<TKey, TValue>(this List<(TKey, TValue)> list)
        => list.GetKeyedLookup<(TKey, TValue), TKey>(PairByKeyEqualityComparer<TKey, TValue>.Default);

    public static KeyedLookup<KeyValuePair<TKey, TValue>, TKey> GetKeyedLookup<TKey, TValue>(this List<KeyValuePair<TKey, TValue>> list)
        => list.GetKeyedLookup((IKeyedEqualityComparer<KeyValuePair<TKey, TValue>, TKey>)PairByKeyEqualityComparer<TKey, TValue>.Default);
    */
    public readonly struct Lookup<T>
    {
        private readonly List<T> _list;
        private readonly IEqualityComparer<T> _comparer;

        internal Lookup(List<T> list, IEqualityComparer<T> comparer)
        {
            _list = list;
            _comparer = comparer;
        }

        public List<T> List => _list;

        public int Count => _list.Count;

        public bool TryGetValue(T equalItem, [MaybeNullWhen(false)] out T actualValue)
        {
            ref readonly var find = ref FindFirstRef(equalItem);
            if (Unsafe.IsNullRef(in find)) {
                actualValue = default;
                return false;
            }
            actualValue = find;
            return true;
        }

        public bool Contains(T item)
        {
            return !Unsafe.IsNullRef(in FindFirstRef(item));
        }

        /// <summary>
        /// Remove the first item that equals to <paramref name="item"/>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(T item)
        {
            ref readonly var find = ref FindFirstRef(item);
            if (Unsafe.IsNullRef(in find)) {
                return false;
            }
            var index = _list.AsSpan().OffsetOf(in item);
            _list.RemoveAt(index);
            return true;
        }

        /// <summary>
        /// Remove all items that equals to <paramref name="equalItem"/>
        /// </summary>
        /// <remarks>
        /// This method performs worse than <see cref="Remove(T)"/>, 
        /// its not recommanded to use this if you sure list doesn't
        /// contain repeated elements.
        /// </remarks>
        /// <param name="item"></param>
        public void RemoveAll(T equalItem)
        {
            var comparer = _comparer;
            _list.RemoveAll(v => comparer.Equals(v, equalItem));
        }

        public bool TryAdd(T item)
        {
            ref var find = ref FindFirstRef(item);
            if (Unsafe.IsNullRef(in find)) {
                _list.Add(item);
                return true;
            }
            return false;
        }

        private ref T FindFirstRef(T item)
        {
            foreach (ref var v in _list.AsSpan()) {
                if (_comparer.Equals(v, item))
                    return ref v;
            }
            return ref Unsafe.NullRef<T>();
        }
    }
    /*
    public readonly struct KeyedLookup<T, TKey>
    {
        private readonly List<T> _list;
        private readonly IKeyedEqualityComparer<T, TKey> _comparer;

        internal KeyedLookup(List<T> list, IKeyedEqualityComparer<T, TKey> comparer)
        {
            _list = list;
            _comparer = comparer;
        }

        public List<T> List => _list;

        public int Count => _list.Count;

        public T this[TKey key]
        {
            get {
                ref readonly var find = ref FindFirstRef(key);
                if (Unsafe.IsNullRef(in find)) {
                    TraThrow.KeyNotFound(key);
                    return default;
                }
                return find;
            }
            set {
                ref var find = ref FindFirstRef(key);
                if (Unsafe.IsNullRef(in find)) {
                    _list.Add(value);
                }
                else {
                    find = value;
                }
            }
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out T value)
        {
            ref readonly var find = ref FindFirstRef(key);
            if (Unsafe.IsNullRef(in find)) {
                value = default;
                return false;
            }
            else {
                value = find;
                return true;
            }
        }

        public bool ContainsKey(TKey key)
            => !Unsafe.IsNullRef(in FindFirstRef(key));

        /// <summary>
        /// Remove the first item that matches <paramref name="key"/>
        /// </summary>
        public bool Remove(TKey key)
        {
            ref readonly var find = ref FindFirstRef(key);
            if (Unsafe.IsNullRef(in find)) {
                return false;
            }
            else {
                var index = _list.AsSpan().OffsetOf(in find);
                _list.RemoveAt(index);
                return true;
            }
        }

        /// <summary>
        /// Remove all items that matches <paramref name="key"/>
        /// </summary>
        /// <remarks>
        /// This method performs worse than <see cref="Remove(TKey)"/>,
        /// better not use this if list doesnt contain repeated elements
        /// </remarks>
        public void RemoveAll(TKey key)
        {
            var comparer = _comparer;
            _list.RemoveAll(item => comparer.Equals(item, key));
        }

        /// <summary>
        /// Add <paramref name="item"/> into collection
        /// if <paramref name="key"/> not found in collection.
        /// Caller owns responsibility to ensure item and key is equal with given comparer
        /// </summary>
        public bool TryAdd(TKey key, T item)
        {
            ref readonly var find = ref FindFirstRef(key);
            if (Unsafe.IsNullRef(in find)) {
                _list.Add(item);
                return true;
            }
            else {
                return false;
            }
        }

        private ref T FindFirstRef(TKey key)
        {
            foreach (ref var item in _list.AsSpan()) {
                if (_comparer.Equals(item, key))
                    return ref item;
            }
            return ref Unsafe.NullRef<T>();
        }
    }
    */
}
