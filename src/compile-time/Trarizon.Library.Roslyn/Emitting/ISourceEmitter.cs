using System.CodeDom.Compiler;
using System.IO;

namespace Trarizon.Library.Roslyn.Emitting;
public interface ISourceEmitter
{
    void Emit(IndentedTextWriter writer);
    string GeneratedFileName { get; }
}

public static class ISourceEmitterExtensions
{
    public static string Emit(this ISourceEmitter emitter)
    {
        using var sw = new StringWriter();
        using var writer = new IndentedTextWriter(sw);
        emitter.Emit(writer);
        return sw.ToString();
    }
}
