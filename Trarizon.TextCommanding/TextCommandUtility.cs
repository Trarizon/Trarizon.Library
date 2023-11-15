using Trarizon.TextCommanding.Input;

namespace Trarizon.TextCommanding;
public static class TextCommandUtility
{
    private static char[] _escapeValues = ['"', '"'];
    public static string Unescape(ReadOnlySpan<char> escapedInput)
    {
        Span<char> buffer = stackalloc char[escapedInput.Length];
        int count = 0;
        for (int i = 0; i < escapedInput.Length; i++) {
            if (escapedInput[i..].StartsWith(_escapeValues))
                i++;
            buffer[count++] = escapedInput[i];
        }
        return new string(buffer[..count]);
    }

    public static StringSplitArgs SplitAsArguments(this string input) => input.AsMemory().SplitAsArguments();

    public static StringSplitArgs SplitAsArguments(this ReadOnlyMemory<char> input) => new(input);

    // Return index of right quote or end of span
    internal static int FindFirstRightQuote(ReadOnlySpan<char> input, int index)
    {
        while (index < input.Length) {
            if (input[index] == TCExecution.Quote) {
                if (index + 1 < input.Length && input[index + 1] == TCExecution.Quote) // escape
                    index++;
                else // here 'end' indicates the end quote
                    break;
            }
            index++;
        }
        return index;
    }

    // Return index of first white space or end of span
    internal static int FindFirstWhiteSpace(ReadOnlySpan<char> input, int index)
    {
        while (index < input.Length && !char.IsWhiteSpace(input[index]))
            index++;
        return index;
    }

#if false
    // Return index of end and the index of '=' in result span
    internal static (int End, int Seperator) GetSplitNotInQuote(ReadOnlySpan<char> input, int start)
    {
        int end = start + 1;
        // Find '='
        for (; end < input.Length && char.IsWhiteSpace(input[end]); end++) {
            if (input[end] == TextCommands.PairSeperator) {
                // If value part starts with quote, find end quote
                if (input[end + 1] == TextCommands.Quote)
                    return (FindFirstRightQuote(input, end + 1), end);
                else
                    return (FindFirstWhiteSpace(input, end + 1), end);
            }
        }
        return (end, -1);
    }
#endif
}
