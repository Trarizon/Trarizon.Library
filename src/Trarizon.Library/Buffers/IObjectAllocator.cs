namespace Trarizon.Library.Buffers;
public interface IObjectAllocator<T> where T : class
{
    T Allocate();
    void Release(T item);
}
