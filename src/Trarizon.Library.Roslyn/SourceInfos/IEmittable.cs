using System.CodeDom.Compiler;
using Trarizon.Library.Roslyn.Emitting;

namespace Trarizon.Library.Roslyn.SourceInfos;

public interface ICSharpEmittable
{
    EmitterIndentScope Emit(IndentedTextWriter writer);
}
