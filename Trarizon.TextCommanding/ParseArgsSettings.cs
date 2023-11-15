namespace Trarizon.TextCommanding;
public sealed record class ParseArgsSettings
{
    internal static ParseArgsSettings Shared { get; } = new();

    private string _fullNamePrefix = "--";
    public string FullNamePrefix
    {
        get => _fullNamePrefix;
        set {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Prefix cannot be null or empty");
            _fullNamePrefix = value;
        }
    }

    private string _shortNamePrefix = "-";
    public string ShortNamePrefix
    {
        get => _shortNamePrefix;
        set {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Prefix cannot be null or empty");
            _shortNamePrefix = value;
        }
    }

    public UnknownParameterNameHandling UnknownParameterNameHandling { get; set; }
}

public enum UnknownParameterNameHandling
{
    ThrowException,
    AsValue,
}