using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trarizon.Library.Experimental.Interpreting.CSharp;
internal class CSharpInterpreter
{
    public ScriptOptions Options { get; } = ScriptOptions.Default
        .WithReferences(AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location)))
        .AddReferences(typeof(CSharpInterpreter).Assembly)
        .AddImports("System", "System.Linq");

    public Task<object> InterpretAsync(string code)
    {
        return CSharpScript.EvaluateAsync(code, Options);
    }
}
