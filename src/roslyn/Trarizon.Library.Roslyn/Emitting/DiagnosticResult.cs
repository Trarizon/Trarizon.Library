using System.Collections.Generic;
using Trarizon.Library.Functional.Monads;
using Trarizon.Library.Roslyn.Diagnostics;


namespace Trarizon.Library.Roslyn.Emitting;
public struct DiagnosticResult<T>
{
    private List<DiagnosticData>? _diags;
    
    public Optional<T> Result { get; set; }

    public IReadOnlyList<DiagnosticData> DiagnosticDatas => _diags ?? [];

    public DiagnosticResult(T result)
    {
        Result = result;
    }

    public DiagnosticResult(DiagnosticData diagnostic) : this([diagnostic]) { }

    public DiagnosticResult(IEnumerable<DiagnosticData> diagnostics) : this([.. diagnostics]) { }

    private DiagnosticResult(List<DiagnosticData> diagnostics)
    {
        _diags = diagnostics;
    }

    public static implicit operator DiagnosticResult<T>(T result) => new(result);

    public static implicit operator DiagnosticResult<T>(DiagnosticData data) => new(data);

    public static implicit operator DiagnosticResult<T>(List<DiagnosticData> diagnostics) => new(diagnostics);

    public static DiagnosticResult<T> As(List<DiagnosticData> diagnostics) => new(diagnostics);

    public void AddDiagnostics(params DiagnosticData[] diagnostics)
    {
        if (_diags is null)
            _diags = [.. diagnostics];
        else
            _diags.AddRange(diagnostics);
    }
}
