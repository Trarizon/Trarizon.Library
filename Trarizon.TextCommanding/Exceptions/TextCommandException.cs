namespace Trarizon.TextCommanding.Exceptions;
public class TextCommandException(string message, ExceptionKind kind) : Exception(message)
{
    public ExceptionKind Kind => kind;
}
