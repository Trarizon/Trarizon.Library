namespace Trarizon.Library.Wrappers;
/// <summary>
/// Provide a more readable way to fill an array
/// </summary>
/// <typeparam name="T"></typeparam>
public struct ArrayFiller<T>(int maxLength)
{
    private readonly T[] _array = new T[maxLength];
    private int _length;

    public readonly T this[int index]
    {
        get {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _length);
            return _array[index];
        }
        set {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _length);
            _array[index] = value;
        }
    }

    public readonly int Length => _length;

    public readonly int MaxLength => _array.Length;

    public readonly bool IsFull => _length == _array.Length;

    public readonly T[] Array => _array;

    public readonly Span<T> Span => _array.AsSpan(0, _length);

    public void Add(T item)
    {
        // T[].this[] will throw
        _array[_length++] = item;
    }

    public void RemoveLast()
    {
        if (_length == 0)
            return;
        _length--;
    }

    public void Clear() => _length = 0;
}
