using Microsoft.CodeAnalysis;

namespace Trarizon.Library.GeneratorToolkit.Extensions;
public static class SyntaxExtensions
{
    public static TSyntax? GetParent<TSyntax>(this SyntaxNode node, bool includingSelf = false) where TSyntax : SyntaxNode
    {
        var current = includingSelf ? node : node.Parent;

        while (current != null) {
            if (current is TSyntax result)
                return result;
            current = current.Parent;
        }
        return null;
    }
}
