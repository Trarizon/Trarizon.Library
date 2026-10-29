namespace Trarizon.Library.Playground.Threading;

public readonly struct LinkedCancellationTokenProvider : IDisposable
{
    private readonly CancellationToken _token;
    private readonly CancellationTokenSource? _cts;

    public CancellationToken Token => _token;

    private LinkedCancellationTokenProvider(CancellationToken token)
    {
        _token = token;
    }

    private LinkedCancellationTokenProvider(CancellationTokenSource cts)
    {
        _token = cts.Token;
        _cts = cts;
    }

    public static LinkedCancellationTokenProvider Create(CancellationToken left, CancellationToken right)
    {
        if (!left.CanBeCanceled)
            return new LinkedCancellationTokenProvider(right);
        if (!right.CanBeCanceled)
            return new LinkedCancellationTokenProvider(left);

        if (left.IsCancellationRequested)
            return new LinkedCancellationTokenProvider(left);
        if (right.IsCancellationRequested)
            return new LinkedCancellationTokenProvider(right);

        if (left == right)
            return new LinkedCancellationTokenProvider(left);

        var cts = CancellationTokenSource.CreateLinkedTokenSource(left, right);
        return new LinkedCancellationTokenProvider(cts);
    }

    public void Dispose() => _cts?.Dispose();
}
