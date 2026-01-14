using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Functional;

public static partial class ResultExtensions
{
    public static T GetValueOrThrowError<T, TException>(this in Result<T, TException> self) where TException : Exception
    {
        if (!self.IsSuccess)
            ThrowException(self.Error);
        return self.Value;
    }

#if NET9_0_OR_GREATER

    public static T GetValueOrThrowError<T, TException>(this in RefResult<T, TException> self)
        where T : allows ref struct where TException : Exception
    {
        if (!self.IsSuccess)
            ThrowException(self.Error);
        return self.Value;
    }

#endif

    [DoesNotReturn]
    private static void ThrowException(Exception exception) => throw exception;
}
