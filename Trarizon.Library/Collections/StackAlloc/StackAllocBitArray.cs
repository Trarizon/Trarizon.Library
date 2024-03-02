using System.Diagnostics;

namespace Trarizon.Library.Collections.StackAlloc;
public ref struct StackAllocBitArray(Span<byte> allocatedSpace)
{
    private readonly Span<byte> _bytes = allocatedSpace;

    public const int BitSizeOfByte = 8;

    public readonly int Length => _bytes.Length * BitSizeOfByte;

    public readonly bool this[int index]
    {
        get {
            var (quo, rem) = Math.DivRem(index, BitSizeOfByte);
            return (_bytes[quo] & GetMask(rem)) != 0;
        }
        set {
            var (quo, rem) = Math.DivRem(index, BitSizeOfByte);
            if (value)
                _bytes[quo] |= GetMask(rem);
            else
                _bytes[quo] &= (byte)~GetMask(rem);
        }
    }

    private static byte GetMask(int index)
    {
        Debug.Assert(index < BitSizeOfByte);
        return (byte)(1 << index);
    }

    public static int GetArrayLength(int bitLength)
        => bitLength > 0 ? (bitLength - 1) / BitSizeOfByte + 1 : 0;

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
