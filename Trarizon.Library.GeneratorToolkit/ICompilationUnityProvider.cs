using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Trarizon.Library.GeneratorToolkit;
public interface ICompilationUnitProvider
{
    CompilationUnitSyntax GetCompilationUnitSyntax();
}
