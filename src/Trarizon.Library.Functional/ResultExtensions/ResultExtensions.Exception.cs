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

    public static ref readonly T GetValueRefOrThrowError<T, TException>(this in Result<T, TException> self) where TException : Exception
    {
        if (!self.IsSuccess)
            ThrowException(self.Error);
        return ref self.GetValueRefOrDefaultRef()!;
    }

#if REF_MONAD

    public static T GetValueOrThrowError<T, TException>(this in RefResult<T, TException> self)
        where T : allows ref struct where TException : Exception
    {
        if (!self.IsSuccess)
            ThrowException(self.Error);
        return self.Value;
    }

    public static ref readonly T GetValueRefOrThrowError<T, TException>(this in RefResult<T, TException> self)
        where T : allows ref struct where TException : Exception
    {
        if (!self.IsSuccess)
            ThrowException(self.Error);
        return ref self.GetValueRefOrDefaultRef()!;
    }

#endif

    [DoesNotReturn]
    private static void ThrowException(Exception exception) => throw exception;
}
