using CommunityToolkit.HighPerformance;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
#if NETSTANDARD
using Unsafe = Trarizon.Library.Netstd.NetstdFix_Unsafe;
#endif

namespace Trarizon.Library.Collections;
public static partial class TraCollection
{
    public static Lookup<T> GetLookup<T>(this List<T> list, IEqualityComparer<T>? comparer = null)
        => new(list, comparer ?? EqualityComparer<T>.Default);

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

}
