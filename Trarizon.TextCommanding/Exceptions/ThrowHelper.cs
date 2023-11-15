using System.Diagnostics.CodeAnalysis;
using Trarizon.TextCommanding.Input.Parameters.CommandParameters;

namespace Trarizon.TextCommanding.Exceptions;
internal static class ThrowHelper
{
    [DoesNotReturn]
    public static void ThrowInvalidOperation(string? message = null)
        => throw new InvalidOperationException(message);

    [DoesNotReturn]
    public static void TextCommandInitializeFailed(string message = "")
        => throw new TextCommandException(message, ExceptionKind.TextCommandInitializationException);

    [DoesNotReturn]
    public static void InputParseFailed(string message = "")
        => throw new TextCommandException(message, ExceptionKind.InputParseException);

    [DoesNotReturn]
    public static void InputParameterRepeated(OptionParameter parameter)
        => InputParseFailed($"Input parameter <{parameter.Attribute.DisplayName}> repeated.");
}
