namespace Trarizon.Library.Playground.Threading;

public static class CancellationHelpers
{
    public static LinkedCancellationTokenProvider Linked(CancellationToken left, CancellationToken right)
        => LinkedCancellationTokenProvider.Create(left, right);
}
