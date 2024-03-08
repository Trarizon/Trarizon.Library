using Trarizon.Library.Collections.Helpers.Utilities.Queriers;

namespace Trarizon.Library.Collections.Helpers;
partial class EnumerableQuery
{
    /// <summary>
    /// Split elements sequence into pairs
    /// </summary>
    /// <param name="paddingElement">
    /// Append to the sequence if the last pair cannot be perfectly filled.
    /// </param>
    public static IEnumerable<(T, T)> ChunkPair<T>(this IEnumerable<T> source, T paddingElement)
        => source is IList<T> list ? list.ChunkPairList(paddingElement) : new ChunkPairQuerier<T>(source, paddingElement)!;

    /// <summary>
    /// Split elements sequence into triples
    /// </summary>
    /// <param name="paddingElement">
    /// Append to the sequence if the last pair cannot be perfectly filled.
    /// </param>
    public static IEnumerable<(T, T, T)> ChunkTriple<T>(this IEnumerable<T> source, T paddingElement)
        => source is IList<T> list ? list.ChunkTripleList(paddingElement) : new ChunkTripleQuerier<T>(source, paddingElement)!;

    /// <summary>
    /// Split elements sequence into pairs
    /// </summary>
    public static IEnumerable<(T, T?)> ChunkPair<T>(this IEnumerable<T> source)
        => source is IList<T> list ? list.ChunkPairList() : new ChunkPairQuerier<T>(source, default);

    /// <summary>
    /// Split elements sequence into triples
    /// </summary>
    public static IEnumerable<(T, T?, T?)> ChunkTriple<T>(this IEnumerable<T> source)
        => source is IList<T> list ? list.ChunkTripleList() : new ChunkTripleQuerier<T>(source, default);


    private sealed class ChunkPairQuerier<T>(IEnumerable<T> source, T? paddingElement) : SimpleEnumerationQuerier<T, (T, T?)>(source)
    {
        public override bool MoveNext()
        {
            if (_state == -1) {
                _enumerator = _source.GetEnumerator();
                _state = 0;
            }

            if (!_enumerator!.MoveNext())
                return false;

            _current.Item1 = _enumerator.Current;
            _current.Item2 = _enumerator.MoveNext() ? _enumerator.Current : paddingElement;
            return true;
        }

        protected override EnumerationQuerier<(T, T?)> Clone() => new ChunkPairQuerier<T>(_source, paddingElement);
    }

    private sealed class ChunkTripleQuerier<T>(IEnumerable<T> source, T? paddingElement) : SimpleEnumerationQuerier<T, (T, T?, T?)>(source)
    {
        public override bool MoveNext()
        {
            if (_state == -1) {
                _enumerator = _source.GetEnumerator();
                _state = 0;
            }

            if (!_enumerator!.MoveNext())
                return false;

            _current.Item1 = _enumerator.Current;

            if (_enumerator.MoveNext()) {
                _current.Item2 = _enumerator.Current;
                _current.Item3 = _enumerator.MoveNext() ? _enumerator.Current : paddingElement;
            }
            else {
                _current.Item2 = _current.Item3 = paddingElement;
            }
            return true;
        }

        protected override EnumerationQuerier<(T, T?, T?)> Clone() => new ChunkTripleQuerier<T>(_source, paddingElement);
    }
}
