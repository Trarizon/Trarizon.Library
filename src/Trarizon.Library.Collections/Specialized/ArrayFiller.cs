using CommunityToolkit.Diagnostics;

namespace Trarizon.Library.Collections.Specialized;
public struct ArrayFiller<T>(T[] array, int initCount = 0)
{
    private int _count = initCount;

    public readonly T[] Array => array;
    public readonly int Count => _count;

    public ArrayFiller(int arrayLength, int initCount = 0) :
        this(new T[arrayLength], initCount)
    { }

    public void Add(T item)
    {
        array[_count++] = item;
    }

    public void InsertAt(int index, T item)
    {
        Guard.IsLessThan(_count, array.Length);
        ArrayGrowHelper.ShiftRightForInsert(array, _count, index, 1);
        array[index] = item;
        _count++;
    }

    public void RemoveAt(int index)
    {
        Guard.IsLessThan(index, _count);
        ArrayGrowHelper.ShiftLeftForRemove(array, _count, index, 1);
        _count--;
    }
}
