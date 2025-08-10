using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text;
using Trarizon.Library.Roslyn.Emitting;

namespace Trarizon.Library.Roslyn.SourceInfos.Emitting;
public static class EmitExtensions
{
    public static IndentedTextWriterExtensions.DeferDedents EmitCSharpPartialTypeHierarchy(this IndentedTextWriter writer, TypeHierarchyInfo? type)
    {
        if (type is null)
            return default;

        var cur = type;
        var stack = new Stack<TypeHierarchyInfo>();
        while (cur is not null) {
            stack.Push(cur);
            cur = cur.Parent;
        }
        var defer = writer.EnterIndentTrackingScope();
        foreach (var t in stack) {
            if (t.IsNamespace) {
                defer.Writer.WriteLine($"{t.Keywords} {t.Name}");
            }
            else {
                defer.Writer.WriteLine($"partial {t.Keywords} {t.Name}");
            }
            defer.WriteBracketAndIndent('{');
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
