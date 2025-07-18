﻿using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Collections;
public static partial class TraCollection
{
    public static ListLookup<T, EqualityComparer<T>> GetLookup<T>(this List<T> list)
        => new(list, EqualityComparer<T>.Default);

    public static ListLookup<T, TComparer> GetLookup<T, TComparer>(this List<T> list, TComparer comparer) where TComparer : IEqualityComparer<T>
        => new(list, comparer);

    public readonly struct ListLookup<T, TComparer> where TComparer : IEqualityComparer<T>
    {
        private readonly List<T> _list;
        private readonly TComparer _comparer;

        internal ListLookup(List<T> list, TComparer comparer)
        {
            _list = list;
            _comparer = comparer;
        }

        public List<T> List => _list;

        public int Count => _list.Count;

        public bool TryGetValue(T equalItem, [MaybeNullWhen(false)] out T actualValue)
        {
            ref var find = ref FindFirstRef(equalItem);
            if (Unsafe.IsNullRef(ref find)) {
                actualValue = default;
                return false;
            }
            actualValue = find;
            return true;
        }

        public bool Contains(T item)
        {
            return !Unsafe.IsNullRef(ref FindFirstRef(item));
        }

        /// <summary>
        /// Remove the first item that equals to <paramref name="item"/>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(T item)
        {
            ref var find = ref FindFirstRef(item);
            if (Unsafe.IsNullRef(ref find)) {
                return false;
            }
            var index = _list.AsListSpan().OffsetOf(in item);
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
            if (Unsafe.IsNullRef(ref find)) {
                _list.Add(item);
                return true;
            }
            return false;
        }

        private ref T FindFirstRef(T item)
        {
            foreach (ref var v in _list.AsListSpan()) {
                if (_comparer.Equals(v, item))
                    return ref v;
            }
            return ref Unsafe.NullRef<T>();
        }
    }

}
