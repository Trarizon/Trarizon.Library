﻿namespace Trarizon.Library.Collections.StackAlloc;
/// <summary>
/// Stack allocated bitarray, use
/// <c>new StackAllocBitArray(stackalloc byte[StackAllocBitArray.GetArrayLength(...)])</c>
/// to create a new stackalloc instance
/// </summary>
/// <param name="allocatedSpace"></param>
public ref struct StackAllocBitArray(Span<uint> allocatedSpace)
{
    private const int BitContainterSize = 8 * sizeof(uint);

    private readonly Span<uint> _span = allocatedSpace;

    public readonly int Length => _span.Length * BitContainterSize;

    public readonly bool this[int index]
    {
        get {
            var (i, mask) = GetSpanIndexAndMask(index);
            return (_span[i] & mask) != 0;
        }
        set {
            var (i, mask) = GetSpanIndexAndMask(index);

            if (value)
                _span[i] |= mask;
            else
                _span[i] &= ~mask;
        }
    }

    private static (int SpanIndex, uint Mask) GetSpanIndexAndMask(int index)
    {
        var quo = Math.DivRem(index, BitContainterSize, out var rem);
        return (quo, 1u << rem);
    }

    /// <summary>
    /// Get the length of <see cref="BitContainer"/> array that caller should allocate
    /// </summary>
    /// <param name="bitLength"></param>
    /// <returns></returns>
    public static int GetArrayLength(int bitLength)
        => bitLength > 0 ? (bitLength - 1) / BitContainterSize + 1 : 0;

    public readonly Enumerator GetEnumerator() => new(this);

    public ref struct Enumerator
    {
        private readonly StackAllocBitArray _array;
        private int _index;

        internal Enumerator(StackAllocBitArray bitArray)
        {
            _array = bitArray;
            _index = -1;
        }

        public readonly bool Current => _array[_index];

        public bool MoveNext()
        {
            var index = _index + 1;
            if (index < _array.Length) {
                _index = index;
                return true;
            }
            return false;
        }
    }
}
