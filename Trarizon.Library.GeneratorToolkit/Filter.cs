using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.GeneratorToolkit;
public sealed class Filter<TContext>
{
    public TContext? Context { get; }
    public IEnumerable<Diagnostic>? Diagnostics { get; }

    [MemberNotNullWhen(false, nameof(Context))]
    [MemberNotNullWhen(true, nameof(Diagnostics))]
    public bool HasDiagnostic => Diagnostics != null && Diagnostics.Any();

    public Filter(TContext context) { Context = context; }

    public Filter(IEnumerable<Diagnostic> diagnostics) {  Diagnostics = diagnostics; }
}
