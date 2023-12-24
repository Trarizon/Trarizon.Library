using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Trarizon.Library.GeneratorToolkit.Extensions;
public static class SyntaxExtensions
{
    /// <summary>
    /// Get the identifier token of type declaration, in other word,
    /// <c>ClassName</c> of <c>public class ClassName {}</c>
    /// </summary>
    public static SyntaxToken TypeIdentifierToken(this TypeDeclarationSyntax typeDeclarationSyntax)
        => typeDeclarationSyntax.ChildTokens().First(token => token.IsKind(SyntaxKind.IdentifierToken));

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
