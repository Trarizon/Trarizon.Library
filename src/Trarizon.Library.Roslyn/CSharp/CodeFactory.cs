namespace Trarizon.Library.Roslyn.CSharp;

public static partial class CodeFactory
{
    #region Literal

    public static string? Literal(object value) => value switch
    {
        string s => Literal(s),
        bool b => Literal(b),
        char c => Literal(c),
        int i => Literal(i),
        uint u => Literal(u),
        long l => Literal(l),
        ulong ul => Literal(ul),
        float f => Literal(f),
        double d => Literal(d),
        _ => null
    };

    public static string Literal(string value) => Fmt($"\"{value}\"");
    public static string LiteralUtf8(string value) => Fmt($"\"{value}\"u8");
    public static string Literal(bool value) => value ? "true" : "false";
    public static string Literal(char value) => Fmt($"'{value}'");
    public static string Literal(int value) => value.ToString();
    public static string Literal(uint value) => Fmt(null, stackalloc char[11], $"{value}u");
    public static string Literal(long value) => Fmt(null, stackalloc char[20], $"{value}L");
    public static string Literal(ulong value) => Fmt(null, stackalloc char[20], $"{value}ul");
    public static string Literal(float value) => Fmt($"{value}f");
    public static string Literal(double value) => value.ToString();

    #endregion Literal

    /// <summary>
    /// <c>#pragma warning disable/restore {err-codes}</c>
    /// </summary>
    public static string PragmaWarningTrivia(bool restore, params ReadOnlySpan<string> errorCodes)
    {
        return Fmt($"#pragma warning {(restore ? "restore" : "disable")} {string.Join(", ", errorCodes)}");
    }
}
