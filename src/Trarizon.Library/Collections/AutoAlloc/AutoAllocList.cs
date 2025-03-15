using Trarizon.Library.Buffers;

namespace Trarizon.Library.Collections.AutoAlloc;
public class AutoAllocList<T>(IObjectAllocator<T> allocator) :
    AutoAllocList<T, IObjectAllocator<T>>(allocator)
    where T : class
{ }

public class AutoAllocList<T, TAllocator>(TAllocator allocator) : BaseAutoAllocList<T>
    where T : class
    where TAllocator : IObjectAllocator<T>
{
    protected override T Allocate() => allocator.Allocate();
    protected override void Release(T item) => allocator.Release(item);
}
