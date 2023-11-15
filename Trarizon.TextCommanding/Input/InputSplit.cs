using System.Diagnostics;

namespace Trarizon.TextCommanding.Input;
[DebuggerDisplay($"{{{nameof(Value)}}}")]
public readonly ref struct InputSplit
{
    public readonly ReadOnlySpan<char> Value;

    internal InputSplit(scoped in ReadOnlySpan<char> span, bool escaped)
    {
        Value = escaped ? TextCommandUtility.Unescape(span) : span;
    }
}
