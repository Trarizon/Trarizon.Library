using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Trarizon.Library.Roslyn.Pipeline;

namespace Trarizon.Test.UnitTests.Roslyn;

public class TypeHierarchyInfoTest
{
    private static (ITypeSymbol Symbol, TypeDeclarationSyntax Syntax) GetTypeInfo(string code, string typeName)
    {
        var tree = CSharpSyntaxTree.ParseText(code);
        var compilation = CSharpCompilation.Create("TestAssembly")
            .AddSyntaxTrees(tree)
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

        var model = compilation.GetSemanticModel(tree);
        var syntax = tree.GetRoot()
            .DescendantNodes()
            .OfType<TypeDeclarationSyntax>()
            .First(t => t.Identifier.Text == typeName);

        var symbol = model.GetDeclaredSymbol(syntax)!;
        return (symbol, syntax);
    }

    [Fact]
    public void Create_SimpleClassInNamespace_ReturnsCorrectHierarchy()
    {
        const string code = @"
namespace TestNamespace
{
    class MyClass { }
}";
        var (symbol, syntax) = GetTypeInfo(code, "MyClass");

        var result = TypeHierarchyInfo.Create(symbol, syntax);

        result.Name.Should().Be("MyClass");
        result.Keywords.Should().Be("class");
        result.Namespace.Should().Be("TestNamespace");
        result.IsNamespace.Should().BeFalse();
        result.Parent.Should().NotBeNull();
        result.Parent!.Name.Should().Be("TestNamespace");
        result.Parent.Keywords.Should().Be("namespace");
        result.Parent.Namespace.Should().Be("TestNamespace");
        result.Parent.IsNamespace.Should().BeTrue();
        result.Parent.Parent.Should().BeNull();
    }

    [Fact]
    public void Create_ClassInGlobalNamespace_ReturnsNullParent()
    {
        const string code = @"
class GlobalClass { }";
        var (symbol, syntax) = GetTypeInfo(code, "GlobalClass");

        var result = TypeHierarchyInfo.Create(symbol, syntax);

        result.Name.Should().Be("GlobalClass");
        result.Keywords.Should().Be("class");
        result.Namespace.Should().BeNull();
        result.IsNamespace.Should().BeFalse();
        result.Parent.Should().BeNull();
    }

    [Fact]
    public void Create_NestedClass_ReturnsCorrectParentChain()
    {
        const string code = @"
namespace NS
{
    class Outer
    {
        class Inner { }
    }
}";
        var (symbol, syntax) = GetTypeInfo(code, "Inner");

        var result = TypeHierarchyInfo.Create(symbol, syntax);

        result.Name.Should().Be("Inner");
        result.Keywords.Should().Be("class");
        result.Namespace.Should().Be("NS");

        result.Parent.Should().NotBeNull();
        result.Parent!.Name.Should().Be("Outer");
        result.Parent.Keywords.Should().Be("class");
        result.Parent.Namespace.Should().Be("NS");

        result.Parent.Parent.Should().NotBeNull();
        result.Parent.Parent!.Name.Should().Be("NS");
        result.Parent.Parent.Keywords.Should().Be("namespace");
        result.Parent.Parent.IsNamespace.Should().BeTrue();
        result.Parent.Parent.Parent.Should().BeNull();
    }

    [Fact]
    public void Create_RecordClass_ReturnsCorrectKeyword()
    {
        const string code = @"
namespace NS
{
    record class MyRecord { }
}";
        var (symbol, syntax) = GetTypeInfo(code, "MyRecord");

        var result = TypeHierarchyInfo.Create(symbol, syntax);

        result.Name.Should().Be("MyRecord");
        result.Keywords.Should().Be("record class");
        result.IsNamespace.Should().BeFalse();
    }

    [Fact]
    public void Create_RecordStruct_ReturnsCorrectKeyword()
    {
        const string code = @"
namespace NS
{
    record struct MyRecordStruct { }
}";
        var (symbol, syntax) = GetTypeInfo(code, "MyRecordStruct");

        var result = TypeHierarchyInfo.Create(symbol, syntax);

        result.Name.Should().Be("MyRecordStruct");
        result.Keywords.Should().Be("record struct");
        result.IsNamespace.Should().BeFalse();
    }

    [Fact]
    public void Create_Struct_ReturnsCorrectKeyword()
    {
        const string code = @"
namespace NS
{
    struct MyStruct { }
}";
        var (symbol, syntax) = GetTypeInfo(code, "MyStruct");

        var result = TypeHierarchyInfo.Create(symbol, syntax);

        result.Name.Should().Be("MyStruct");
        result.Keywords.Should().Be("struct");
        result.IsNamespace.Should().BeFalse();
    }

    [Fact]
    public void Create_Interface_ReturnsCorrectKeyword()
    {
        const string code = @"
namespace NS
{
    interface IMyInterface { }
}";
        var (symbol, syntax) = GetTypeInfo(code, "IMyInterface");

        var result = TypeHierarchyInfo.Create(symbol, syntax);

        result.Name.Should().Be("IMyInterface");
        result.Keywords.Should().Be("interface");
        result.IsNamespace.Should().BeFalse();
    }

    [Fact]
    public void Create_GenericClass_IncludesTypeParametersInName()
    {
        const string code = @"
namespace NS
{
    class Generic<T, U> { }
}";
        var (symbol, syntax) = GetTypeInfo(code, "Generic");

        var result = TypeHierarchyInfo.Create(symbol, syntax);

        result.Name.Should().Be("Generic<T, U>");
        result.Keywords.Should().Be("class");
        result.Namespace.Should().Be("NS");
    }

    [Fact]
    public void Create_NestedRecordInClass_ReturnsCorrectHierarchy()
    {
        const string code = @"
namespace NS
{
    class Outer
    {
        record struct Inner(int X, int Y);
    }
}";
        var (symbol, syntax) = GetTypeInfo(code, "Inner");

        var result = TypeHierarchyInfo.Create(symbol, syntax);

        result.Name.Should().Be("Inner");
        result.Keywords.Should().Be("record struct");
        result.Namespace.Should().Be("NS");

        result.Parent.Should().NotBeNull();
        result.Parent!.Name.Should().Be("Outer");
        result.Parent.Keywords.Should().Be("class");
        result.Parent.Namespace.Should().Be("NS");

        result.Parent.Parent.Should().NotBeNull();
        result.Parent.Parent!.Name.Should().Be("NS");
        result.Parent.Parent.Keywords.Should().Be("namespace");
        result.Parent.Parent.IsNamespace.Should().BeTrue();
    }

    [Fact]
    public void Create_DeeplyNested_ReturnsFullChain()
    {
        const string code = @"
namespace A.B.C
{
    class L1
    {
        class L2
        {
            struct L3 { }
        }
    }
}";
        var (symbol, syntax) = GetTypeInfo(code, "L3");

        var result = TypeHierarchyInfo.Create(symbol, syntax);

        result.Name.Should().Be("L3");
        result.Keywords.Should().Be("struct");

        result.Parent.Should().NotBeNull();
        result.Parent!.Name.Should().Be("L2");
        result.Parent.Keywords.Should().Be("class");

        result.Parent.Parent.Should().NotBeNull();
        result.Parent.Parent!.Name.Should().Be("L1");
        result.Parent.Parent.Keywords.Should().Be("class");

        result.Parent.Parent.Parent.Should().NotBeNull();
        result.Parent.Parent.Parent!.Name.Should().Be("A.B.C");
        result.Parent.Parent.Parent.Keywords.Should().Be("namespace");
        result.Parent.Parent.Parent.IsNamespace.Should().BeTrue();
        result.Parent.Parent.Parent.Parent.Should().BeNull();
    }

    [Fact]
    public void Create_NestedGenericClass_IncludesTypeParameters()
    {
        const string code = @"
namespace NS
{
    class Outer<T>
    {
        class Inner<U> { }
    }
}";
        var (symbol, syntax) = GetTypeInfo(code, "Inner");

        var result = TypeHierarchyInfo.Create(symbol, syntax);

        result.Name.Should().Be("Inner<U>");
        result.Parent.Should().NotBeNull();
        result.Parent!.Name.Should().Be("Outer<T>");
    }

    [Fact]
    public void Create_RecordWithoutClassOrStructKeyword_ReturnsRecordKeyword()
    {
        const string code = @"
namespace NS
{
    record MyRecord { }
}";
        var (symbol, syntax) = GetTypeInfo(code, "MyRecord");

        var result = TypeHierarchyInfo.Create(symbol, syntax);

        result.Name.Should().Be("MyRecord");
        result.Keywords.Should().Be("record");
    }
}