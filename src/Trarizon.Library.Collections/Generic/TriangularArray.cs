using System.Collections;
using System.Diagnostics;
using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Library.Collections.Generic;
public readonly struct TriangularArray<T>
{
    private readonly T[] _array;
    private readonly int _level;

    public TriangularArray(int levelCount)
    {
        _level = levelCount;
        _array = new T[_level * (_level + 1) / 2];
    }

    public ref T this[int levelIndex, int itemIndex]
    {
        get {
            Throws.ThrowIfIndexGreaterThanOrEqual(levelIndex, _level);
            Throws.ThrowIfGreaterThan((uint)itemIndex, (uint)levelIndex);

            if (levelIndex == 0) {
                Debug.Assert(itemIndex == 0);
                return ref _array[0];
            }

            int start = (1 + levelIndex) * levelIndex / 2;
            return ref _array[start + itemIndex];
        }
    }

    public int LevelCount => _level;

    public int Count => _array.Length;

    public Span<T> AsLevelSpan(int levelIndex)
    {
        Throws.ThrowIfIndexGreaterThanOrEqual(levelIndex, _level);

        if (levelIndex == 0)
            return _array.AsSpan(0, 1);

        var start = (1 + levelIndex) * levelIndex / 2;
        return _array.AsSpan(start, levelIndex + 1);
    }

    public Span<T> AsSpan() => _array.AsSpan();

    public LevelIterator EnumerateLevelSpans() => new LevelIterator(this);

    public struct LevelIterator
#if NET9_0_OR_GREATER
        : IEnumerable<Span<T>>
        , IEnumerator<Span<T>>
#endif
    {
        private readonly TriangularArray<T> _array;
        private int _index;
        private int _start;

        internal LevelIterator(TriangularArray<T> array)
        {
            _array = array;
        }

        public readonly Span<T> Current => _array._array.AsSpan(_start, _index);

        public bool MoveNext()
        {
            if (_index >= _array._level)
                return false;

            _start += _index;
            _index++;
            return true;
        }

        public readonly LevelIterator GetEnumerator() => new(_array);

#if NET9_0_OR_GREATER

        object IEnumerator.Current { get { Throws.ThrowInvalidOperation(); return default!; } }

        IEnumerator<Span<T>> IEnumerable<Span<T>>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        void IEnumerator.Reset() { }
        void IDisposable.Dispose() { }

#endif
    }
}
