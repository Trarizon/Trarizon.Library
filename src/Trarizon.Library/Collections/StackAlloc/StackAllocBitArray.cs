using System.Diagnostics;

namespace Trarizon.Library.Collections.StackAlloc;
/// <summary>
/// Stack allocated bitarray, use
/// <c>new StackAllocBitArray(stackalloc byte[StackAllocBitArray.GetArrayLength(...)])</c>
/// to create a new stackalloc instance
/// </summary>
/// <param name="allocatedSpace"></param>
public ref struct StackAllocBitArray(Span<byte> allocatedSpace)
{
    private const int BitContainterSize = 8 * sizeof(byte);

    private readonly Span<byte> _span = allocatedSpace;

    public readonly int Length => _span.Length * BitContainterSize;

    public readonly bool this[int index]
    {
        get {
            var (quo, rem) = int.DivRem(index, BitContainterSize);
            return (_span[quo] & GetMask(rem)) != 0;
        }
        set {
            var (quo, rem) = int.DivRem(index, BitContainterSize);
            if (value)
                _span[quo] |= GetMask(rem);
            else
                _span[quo] &= (byte)~GetMask(rem);
        }
    }

    /// <summary>
    /// Get the length of <see cref="BitContainer"/> array that caller should allocate
    /// </summary>
    /// <param name="bitLength"></param>
    /// <returns></returns>
    public static int GetArrayLength(int bitLength)
        => bitLength > 0 ? (bitLength - 1) / BitContainterSize + 1 : 0;

    private static byte GetMask(int index)
    {
        Debug.Assert(index < BitContainterSize);
        return (byte)(1 << index);
    }

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
