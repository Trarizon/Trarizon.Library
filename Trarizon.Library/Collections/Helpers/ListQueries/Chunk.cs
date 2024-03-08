using Trarizon.Library.Collections.Helpers.Utilities.Queriers;
using Trarizon.Library.Collections.Helpers.ListQueries.Helpers;

namespace Trarizon.Library.Collections.Helpers;
partial class ListQuery
{
    /// <summary>
    /// Split elements sequence into pairs
    /// </summary>
    /// <param name="paddingElement">
    /// Append to the sequence if the last pair cannot be perfectly filled.
    /// </param>
    public static IList<(T, T)> ChunkPairList<T>(this IList<T> list, T paddingElement)
        => new ChunkPairQuerier<ListWrapper<T>, T>(list.Wrap(), paddingElement)!;

    /// <summary>
    /// Split elements sequence into pairs
    /// </summary>
    /// <param name="paddingElement">
    /// Append to the sequence if the last pair cannot be perfectly filled.
    /// </param>
    public static IReadOnlyList<(T, T)> ChunkPairROList<T>(this IReadOnlyList<T> list, T paddingElement)
        => new ChunkPairQuerier<IReadOnlyList<T>, T>(list, paddingElement)!;

    /// <summary>
    /// Split elements sequence into triples
    /// </summary>
    /// <param name="paddingElement">
    /// Append to the sequence if the last pair cannot be perfectly filled.
    /// </param>
    public static IList<(T, T, T)> ChunkTripleList<T>(this IList<T> list, T paddingElement)
        => new ChunkTripleQuerier<ListWrapper<T>, T>(list.Wrap(), paddingElement)!;

    /// <summary>
    /// Split elements sequence into triples
    /// </summary>
    /// <param name="paddingElement">
    /// Append to the sequence if the last pair cannot be perfectly filled.
    /// </param>
    public static IReadOnlyList<(T, T, T)> ChunkTripleROList<T>(this IReadOnlyList<T> list, T paddingElement)
        => new ChunkTripleQuerier<IReadOnlyList<T>, T>(list, paddingElement)!;

    /// <summary>
    /// Split elements sequence into pairs
    /// </summary>
    public static IList<(T, T?)> ChunkPairList<T>(this IList<T> list)
        => new ChunkPairQuerier<ListWrapper<T>, T>(list.Wrap(), default)!;

    /// <summary>
    /// Split elements sequence into pairs
    /// </summary>
    public static IReadOnlyList<(T, T?)> ChunkPairROList<T>(this IReadOnlyList<T> list)
        => new ChunkPairQuerier<IReadOnlyList<T>, T>(list, default)!;

    /// <summary>
    /// Split elements sequence into triples
    /// </summary>
    public static IList<(T, T?, T?)> ChunkTripleList<T>(this IList<T> list)
        => new ChunkTripleQuerier<ListWrapper<T>, T>(list.Wrap(), default)!;

    /// <summary>
    /// Split elements sequence into triples
    /// </summary>
    public static IReadOnlyList<(T, T?, T?)> ChunkTripleList<T>(this IReadOnlyList<T> list)
        => new ChunkTripleQuerier<IReadOnlyList<T>, T>(list, default)!;


    private sealed class ChunkPairQuerier<TList, T>(TList list, T? paddingElement) : SimpleListQuerier<TList, T, (T, T?)>(list) where TList : IReadOnlyList<T>
    {
        public override (T, T?) this[int index]
        {
            get {
                var i = index * 2;
                return (_list[i],
                    ElementAtOrDefaultOpt(_list, index + 1, paddingElement));
            }
        }

        public override int Count => (_list.Count + 1) / 2;

        protected override EnumerationQuerier<(T, T?)> Clone() => new ChunkPairQuerier<TList, T>(_list, paddingElement);
    }

    private sealed class ChunkTripleQuerier<TList, T>(TList list, T? paddingElement) : SimpleListQuerier<TList, T, (T, T?, T?)>(list) where TList : IReadOnlyList<T>
    {
        public override (T, T?, T?) this[int index]
        {
            get {
                var i = index * 2;
                return (_list[i],
                    ElementAtOrDefaultOpt(_list, i + 1, paddingElement),
                    ElementAtOrDefaultOpt(_list, i + 2, paddingElement));
            }
        }

        public override int Count => (_list.Count + 1) / 3;

        protected override EnumerationQuerier<(T, T?, T?)> Clone() => new ChunkTripleQuerier<TList, T>(_list, paddingElement);
    }
}
