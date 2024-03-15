using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Trarizon.Library.SourceGenerator.Toolkit.Extensions;
internal static class SyntaxExtensions
{
    public static AccessModifiers GetAccessModifiers(this MemberDeclarationSyntax memberDeclarationSyntax)
    {
        AccessModifiers modifiers = AccessModifiers.None;
        foreach (var modifier in memberDeclarationSyntax.Modifiers) {
            switch ((SyntaxKind)modifier.RawKind) {
                case SyntaxKind.PublicKeyword:
                    modifiers |= AccessModifiers.Public;
                    break;
                case SyntaxKind.ProtectedKeyword:
                    modifiers |= AccessModifiers.Protected;
                    break;
                case SyntaxKind.InternalKeyword:
                    modifiers |= AccessModifiers.Internal;
                    break;
                case SyntaxKind.PrivateKeyword:
                    modifiers |= AccessModifiers.Private;
                    break;
                case SyntaxKind.FileKeyword:
                    modifiers |= AccessModifiers.File;
                    break;
            }
        }
        return modifiers;
    }
}
