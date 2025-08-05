using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text;
using Trarizon.Library.Roslyn.SourceInfos.Emitting;

namespace Trarizon.Library.Roslyn.SourceInfos.CSharp.Emitting;
public static class EmitExtensions
{
    public static IndentedTextWriterExtensions.DeferDedents EmitCSharpNamespace(this IndentedTextWriter writer, string? @namespace)
    {
        if (@namespace is null) {
            return default;
        }

        writer.WriteLine($"namespace {@namespace}");
        return writer.EnterBracketDedentScope('{');
    }

    public static IndentedTextWriterExtensions.DeferDedents EmitCSharpNamespace(this IndentedTextWriter writer, NamespaceDeclarationCodeInfo ns)
    {
        return ns.EmitCSharp(writer);
    }

    public static IndentedTextWriterExtensions.DeferDedents EmitCSharpPartialTypeAndContainingTypeAndNamespaces(this IndentedTextWriter writer, NamespaceOrTypeDeclarationCodeInfo? nsType)
    {
        if (nsType is null)
            return default;

        var cur = nsType;
        var stack = new Stack<NamespaceOrTypeDeclarationCodeInfo>();
        while (cur is not null) {
            stack.Push(cur);
            cur = cur.Parent;
        }

        var defer = writer.EnterIndentTrackingScope();
        foreach (var nst in stack) {
            defer.Append(nst.EmitCSharp);
        }

        return defer.ToDeferDedents();
    }

    public static void EmitCSharpXmlDocLine(this IndentedTextWriter writer, string text, bool autoEscape = false)
    {
        if (autoEscape)
            text = Escape(text);
        writer.WriteLine($"/// {text}");

        static string Escape(string text)
        {
            //                                       guessLength
            var sb = new StringBuilder(text.Length + 6);
            foreach (var c in text) {
                if (c is '<')
                    sb.Append("&lt;");
                else if (c is '>')
                    sb.Append("&gt;");
                else if (c is '&')
                    sb.Append("&amp;");
                else if (c is '\n')
                    sb.Append("<br/>");
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
