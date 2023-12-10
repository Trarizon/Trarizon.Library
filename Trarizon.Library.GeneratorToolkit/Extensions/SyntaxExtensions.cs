using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Trarizon.Library.GeneratorToolkit.Extensions;
public static class SyntaxExtensions
{
    public static SyntaxToken[] ToTokens(this SyntaxKind[] kinds)
    {
        var tokens = new SyntaxToken[kinds.Length];
        for (int i = 0; i < kinds.Length; i++) {
            tokens[i] = SyntaxFactory.Token(kinds[i]);
        }
        return tokens;
    }

    /// <summary>
    /// Get the identifier token of type declaration, in other word,
    /// <c>ClassName</c> of <c>public class ClassName {}</c>
    /// </summary>
    public static SyntaxToken TypeIdentifierToken(this TypeDeclarationSyntax typeDeclarationSyntax)
        => typeDeclarationSyntax.ChildTokens().First(token => token.IsKind(SyntaxKind.IdentifierToken));
}
