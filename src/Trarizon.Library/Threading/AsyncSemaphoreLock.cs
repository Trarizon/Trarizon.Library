namespace Trarizon.Library.Threading;
public sealed class AsyncSemaphoreLock
{
    private readonly SemaphoreSlim _semaphore = new(0, 1);

    public async Task<Scope> EnterAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        return new Scope(this);
    }

    public void Exit()
    {
        _semaphore.Release();
    }

    public readonly struct Scope : IDisposable
    {
        private readonly AsyncSemaphoreLock _lock;

        internal Scope(AsyncSemaphoreLock @lock) => _lock = @lock;

        public void Dispose() => _lock?.Exit();
    }
}
