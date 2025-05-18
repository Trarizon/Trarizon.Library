using System.Collections.Generic;
using Trarizon.Library.Functional.Monads;


namespace Trarizon.Library.Roslyn.Emitting;
public struct ParseResult<T>
{
    private List<DiagnosticData>? _diags;
    
    public Optional<T> Result { get; set; }

    public IReadOnlyList<DiagnosticData> DiagnosticDatas => _diags ?? [];

    public ParseResult(T result)
    {
        Result = result;
    }

    public ParseResult(DiagnosticData diagnostic) : this([diagnostic]) { }

    public ParseResult(IEnumerable<DiagnosticData> diagnostics) : this([.. diagnostics]) { }

    private ParseResult(List<DiagnosticData> diagnostics)
    {
        _diags = diagnostics;
    }

    public static implicit operator ParseResult<T>(T result) => new(result);

    public static implicit operator ParseResult<T>(DiagnosticData data) => new(data);

    public static implicit operator ParseResult<T>(List<DiagnosticData> diagnostics) => new(diagnostics);

    public static ParseResult<T> As(List<DiagnosticData> diagnostics) => new(diagnostics);

    public void AddDiagnostics(params DiagnosticData[] diagnostics)
    {
        if (_diags is null)
            _diags = [.. diagnostics];
        else
            _diags.AddRange(diagnostics);
    }
}
