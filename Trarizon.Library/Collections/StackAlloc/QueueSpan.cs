using Trarizon.Library.CodeAnalysis.MemberAccess;
using Trarizon.Library.Collections.AllocOpt;

namespace Trarizon.Library.Collections.StackAlloc;
public readonly ref struct QueueSpan<T>
{
    private readonly Span<T> _firstPart;
    private readonly Span<T> _secondPart;

    [FriendAccess(typeof(AllocOptQueue<>))]
    internal QueueSpan(T[] underlyingArray, int head, int tail)
    {
        if (tail > head) {
            _firstPart = underlyingArray.AsSpan(head..tail);
            _secondPart = [];
        }
        else {
            _firstPart = underlyingArray.AsSpan(head);
            _secondPart = underlyingArray.AsSpan(0, tail);
        }
    }

    private QueueSpan(Span<T> firstPart, Span<T> secondPart)
    {
        _firstPart = firstPart;
        _secondPart = secondPart;
    }

    public ref T this[int index]
    {
        get {
            if (index < _firstPart.Length)
                return ref _firstPart[index];
            index -= _firstPart.Length;
            // _secondPart.Slice() will throw if out of range
            return ref _secondPart[index];
        }
    }

    public QueueSpan<T> Slice(int startIndex, int length)
    {
        if (startIndex < _firstPart.Length) {
            var endIndex = startIndex + length;
            if (endIndex < _firstPart.Length)
                return new(_firstPart.Slice(startIndex, length), []);
            endIndex -= _firstPart.Length;
            // _secondPart.Slice() will throw if out of range
            return new(_firstPart, _secondPart[..endIndex]);
        }
        startIndex -= _firstPart.Length;
        // _secondPart.Slice() will throw if out of range
        return new(_secondPart.Slice(startIndex, length), []);
    }
}
