namespace Trarizon.Library.Collections.StackAlloc;
public ref struct StackAllocBitArray(Span<byte> allocatedSpace)
{
    private readonly Span<byte> _bytes = allocatedSpace;

    public const int BitSizeOfByte = 8;

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

    private static byte GetMask(int index) => (byte)(1 << index);

    public static int GetArrayLength(int bitLength)
        => bitLength > 0 ? (bitLength - 1) / BitSizeOfByte + 1 : 0;
}
