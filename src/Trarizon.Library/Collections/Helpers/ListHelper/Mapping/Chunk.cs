﻿using Trarizon.Library.Collections.Helpers.Queriers;

namespace Trarizon.Library.Collections.Helpers;
partial class ListHelper
{
    /// <summary>
    /// Split elements sequence into pairs
    /// </summary>
    /// <param name="paddingElement">
    /// Append to the sequence if the last pair cannot be perfectly filled.
    /// </param>
    public static IList<(T, T)> ChunkPairList<T>(this IList<T> list, T paddingElement)
    {
        if (list.IsFixedSizeEmpty())
            return Array.Empty<(T, T)>();

        return new ChunkPairQuerier<IList<T>, T>(list, paddingElement)!;
    }

    /// <summary>
    /// Split elements sequence into pairs
    /// </summary>
    /// <param name="paddingElement">
    /// Append to the sequence if the last pair cannot be perfectly filled.
    /// </param>
    public static IReadOnlyList<(T, T)> ChunkPairROList<T>(this IReadOnlyList<T> list, T paddingElement)
    {
        if (list.IsFixedSizeEmpty())
            return Array.Empty<(T, T)>();

        return new ChunkPairQuerier<ListWrapper<T>, T>(list.Wrap(), paddingElement)!;
    }


    /// <summary>
    /// Split elements sequence into pairs
    /// </summary>
    public static IList<(T, T?)> ChunkPairList<T>(this IList<T> list)
    {
        if (list.IsFixedSizeEmpty())
            return Array.Empty<(T, T?)>();

        return new ChunkPairQuerier<IList<T>, T>(list, default!)!;
    }

    /// <summary>
    /// Split elements sequence into pairs
    /// </summary>
    public static IReadOnlyList<(T, T?)> ChunkPairROList<T>(this IReadOnlyList<T> list)
    {
        if (list.IsFixedSizeEmpty())
            return Array.Empty<(T, T?)>();

        return new ChunkPairQuerier<ListWrapper<T>, T>(list.Wrap(), default!)!;
    }


    /// <summary>
    /// Split elements sequence into triples
    /// </summary>
    /// <param name="paddingElement">
    /// Append to the sequence if the last pair cannot be perfectly filled.
    /// </param>
    public static IList<(T, T, T)> ChunkTripleList<T>(this IList<T> list, T paddingElement)
    {
        if (list.IsFixedSizeEmpty())
            return Array.Empty<(T, T, T)>();

        return new ChunkTripleQuerier<IList<T>, T>(list, paddingElement, paddingElement)!;
    }

    /// <summary>
    /// Split elements sequence into triples
    /// </summary>
    /// <param name="paddingElement">
    /// Append to the sequence if the last pair cannot be perfectly filled.
    /// </param>
    public static IReadOnlyList<(T, T, T)> ChunkTripleROList<T>(this IReadOnlyList<T> list, T paddingElement)
    {
        if (list.IsFixedSizeEmpty())
            return Array.Empty<(T, T, T)>();

        return new ChunkTripleQuerier<ListWrapper<T>, T>(list.Wrap(), paddingElement, paddingElement)!;
    }


    /// <summary>
    /// Split elements sequence into triples
    /// </summary>
    public static IList<(T, T?, T?)> ChunkTripleList<T>(this IList<T> list)
    {
        if (list.IsFixedSizeEmpty())
            return Array.Empty<(T, T?, T?)>();

        return new ChunkTripleQuerier<IList<T>, T>(list, default!, default!)!;
    }

    /// <summary>
    /// Split elements sequence into triples
    /// </summary>
    public static IReadOnlyList<(T, T?, T?)> ChunkTripleROList<T>(this IReadOnlyList<T> list)
    {
        if (list.IsFixedSizeEmpty())
            return Array.Empty<(T, T?, T?)>();

        return new ChunkTripleQuerier<ListWrapper<T>, T>(list.Wrap(), default!, default!)!;
    }


    private sealed class ChunkPairQuerier<TList, T>(TList list, T paddingElem)
        : SimpleListQuerier<TList, T, (T, T)>(list)
        where TList : IList<T>
    {
        private T _paddingElem = paddingElem;

        public override (T, T) this[int index]
        {
            get {
                var i = index * 2;
                T v1 = _list[i];
                TryAtOpt<TList, T>(_list, i + 1, out var v2);
                return (v1, v2)!;
            }
            set {
                var i = index * 2;
                var i2 = i + 1;
                if (i2 < _list.Count)
                    (_list[i], _list[i2]) = value;
                else
                    (_list[i], _paddingElem) = value;
            }
        }

        public override int Count => (_list.Count + 1) / 2;

        protected override EnumerationQuerier<(T, T)> Clone() => new ChunkPairQuerier<TList, T>(_list, _paddingElem);
    }

    private sealed class ChunkTripleQuerier<TList, T>(TList list, T paddingElem1, T paddingElem2)
        : SimpleListQuerier<TList, T, (T, T, T)>(list)
        where TList : IList<T>
    {
        private T _paddingElem1 = paddingElem1;
        private T _paddingElem2 = paddingElem2;

        public override (T, T, T) this[int index]
        {
            get {
                var i = index * 3;
                T v1 = _list[i];
                TryAtOpt<TList, T>(_list, i + 1, out var v2);
                TryAtOpt<TList, T>(_list, i + 2, out var v3);
                return (v1, v2, v3)!;
            }
            set {
                var i = index * 3;
                var count = _list.Count;
                if (i + 2 < count) {
                    (_list[i], _list[i + 1], _list[i + 2]) = value;
                }
                else if (i + 1 < count) {
                    (_list[i], _list[i + 1], _paddingElem2) = value;
                }
                else {
                    (_list[i], _paddingElem1, _paddingElem2) = value;
                }
            }
        }

        public override int Count => (_list.Count + 1) / 3;

        protected override EnumerationQuerier<(T, T, T)> Clone() => new ChunkTripleQuerier<TList, T>(_list, _paddingElem1, _paddingElem2);
    }
}
