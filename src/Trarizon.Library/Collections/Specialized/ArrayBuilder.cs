using CommunityToolkit.Diagnostics;

namespace Trarizon.Library.Collections.Specialized;
public struct ArrayBuilder<T>(T[] array, int initCount = 0)
{
    private int _count = initCount;

    public readonly int Count => _count;

    public void Add(T item)
    {
        array[_count++] = item;
    }

    public void InsertAt(int index, T item)
    {
        Guard.IsLessThan(_count, array.Length);
        Array.Copy(array, index, array, index + 1, _count - index);
        array[index] = item;
        _count++;
    }

    public void RemoveAt(int index)
    {
        Guard.IsLessThan(index, _count);
        Array.Copy(array, index + 1, array, index, _count - index - 1);
        _count--;
        ArrayGrowHelper.FreeManaged(array, _count, 1);
    }
}
