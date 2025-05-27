using Trarizon.Library.Collections.AllocOpt;

namespace Trarizon.Library.Collections;
public static partial class TraEnumerable
{
    public static IEnumerable<(T, T?)> ChunkPair<T>(this IEnumerable<T> source)
    {
        if (source is T[] array)
            return array.Length == 0 ? [] : new ArrayChunkPairIterator<T>(array);
        return Iterate(source);

        static IEnumerable<(T, T?)> Iterate(IEnumerable<T> source)
        {
            using var enumerator = source.GetEnumerator();

            while (enumerator.TryMoveNext(out var left)) {
                if (enumerator.TryMoveNext(out var right))
                    yield return (left, right);
                else {
                    yield return (left, default);
                    yield break;
                }
            }
        }
    }

    public static IEnumerable<(T, T?, T?)> ChunkTriple<T>(this IEnumerable<T> source)
    {
        if (source is T[] array)
            return array.Length == 0 ? [] : new ArrayChunkTripleIterator<T>(array);
        if (source.IsEmptyArray())
            return [];
        return Iterate(source);

        static IEnumerable<(T, T?, T?)> Iterate(IEnumerable<T> source)
        {
            using var enumerator = source.GetEnumerator();

            while (enumerator.TryMoveNext(out var first)) {
                if (!enumerator.TryMoveNext(out var second)) {
                    yield return (first, default, default);
                    yield break;
                }

                if (!enumerator.TryMoveNext(out var third)) {
                    yield return (first, second, default);
                    yield break;
                }

                yield return (first, second, third);
            }
        }
    }

    public static IEnumerable<T[]> ChunkBy<T, TEquatable>(this IEnumerable<T> source, TEquatable seperator, bool ignoreEmptyChunk = false)
        where TEquatable : IEquatable<T>
    {
        if (ignoreEmptyChunk)
            return IterateExcludeEmpty(source, seperator);
        else
            return IterateIncludeEmpty(source, seperator);

        static IEnumerable<T[]> IterateIncludeEmpty(IEnumerable<T> source, TEquatable seperator)
        {
            using var enumerator = source.GetEnumerator();
            using AllocOptList<T> buffer = [];
            while (enumerator.MoveNext()) {
                var current = enumerator.Current;
                if (seperator.Equals(current)) {
                    yield return buffer.ToArray();
                    buffer.Clear();
                }
                else {
                    buffer.Add(current);
                }
            }
            yield return buffer.ToArray();
            buffer.Clear();
        }

        static IEnumerable<T[]> IterateExcludeEmpty(IEnumerable<T> source, TEquatable seperator)
        {
            using var enumerator = source.GetEnumerator();
            using AllocOptList<T> buffer = [];
            while (enumerator.MoveNext()) {
                var current = enumerator.Current;
                if (seperator.Equals(current)) {
                    if (buffer.Count > 0) {
                        yield return buffer.ToArray();
                        buffer.Clear();
                    }
                }
                else {
                    buffer.Add(current);
                }
            }
            if (buffer.Count > 0) {
                yield return buffer.ToArray();
                buffer.Clear();
            }
        }
    }

    private sealed class ArrayChunkPairIterator<T>(T[] array) : ListIteratorBase<(T, T?)>
    {
        private (T, T?) _current;

        public override (T, T?) this[int index]
        {
            get {
                var i = index * 2;
                return (array[i], array.ElementAtOrDefault(i + 1));
            }
        }

        public override int Count => (array.Length + 1) / 2;

        public override (T, T?) Current => _current;

        public override bool MoveNext() => MoveNext_Index(ref _current);
        protected override IteratorBase<(T, T?)> Clone() => new ArrayChunkPairIterator<T>(array);
    }

    private sealed class ArrayChunkTripleIterator<T>(T[] array) : ListIteratorBase<(T, T?, T?)>
    {
        private (T, T?, T?) _current;

        public override (T, T?, T?) this[int index]
        {
            get {
                var i = index * 3;
                return (array[i], array.ElementAtOrDefault(i + 1), array.ElementAtOrDefault(i + 2));
            }
        }

        public override int Count => (array.Length + 2) / 3;

        public override (T, T?, T?) Current => _current;

        public override bool MoveNext() => MoveNext_Index(ref _current);
        protected override IteratorBase<(T, T?, T?)> Clone() => new ArrayChunkTripleIterator<T>(array);
    }
}
