using Trarizon.Library.Buffers;

namespace Trarizon.Library.Collections.AutoAlloc;
public class AutoAllocList<T> : AutoAllocListBase<T> where T : class
{
    private IObjectAllocator<T> _allocator;

    public AutoAllocList(IObjectAllocator<T> allocator)
    {
        _allocator = allocator;
    }

    protected override T Allocate() => _allocator.Allocate();
    protected override void Release(T item) => _allocator.Release(item);
}
