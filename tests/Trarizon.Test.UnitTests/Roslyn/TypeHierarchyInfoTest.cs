using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Trarizon.Library.Roslyn;
using Trarizon.Library.Roslyn.Pipeline;

namespace Trarizon.Test.UnitTests.Roslyn;

public class TypeHierarchyInfoTest
{
    private static INamedTypeSymbol GetTypeSymbol(string code, string typeName)
    {
        var tree = CSharpSyntaxTree.ParseText(code);
        var compilation = CSharpCompilation.Create("TestAssembly")
            .AddSyntaxTrees(tree)
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

        var model = compilation.GetSemanticModel(tree);
        var syntax = tree.GetRoot()
            .DescendantNodes()
            .OfType<BaseTypeDeclarationSyntax>()
            .First(t => t.Identifier.Text == typeName);

        return model.GetDeclaredSymbol(syntax)!;
    }

    [Fact]
    public void Create_SimpleClassInNamespace_ReturnsCorrectHierarchy()
    {
        const string code = @"
namespace TestNamespace
{
    class MyClass { }
}";
        var symbol = GetTypeSymbol(code, "MyClass");

        var result = TypeHierarchyInfo.Create(symbol);

        result.Name.Should().Be("MyClass");
        result.Keywords.Should().Be("class");
        result.Namespace.Should().Be("TestNamespace");
        result.IsNamespace.Should().BeFalse();

        result.Parent.IsNamespace.Should().BeTrue();
        result.Parent.Name.Should().Be("TestNamespace");
        result.Parent.Keywords.Should().Be("namespace");
        result.Parent.Namespace.Should().Be("TestNamespace");

        result.Parent.Parent.Should().Be(default(TypeHierarchyInfo));
    }

    [Fact]
    public void Create_ClassInGlobalNamespace_ParentIsDefault()
    {
        const string code = @"
class GlobalClass { }";
        var symbol = GetTypeSymbol(code, "GlobalClass");

        var result = TypeHierarchyInfo.Create(symbol);

        result.Name.Should().Be("GlobalClass");
        result.Keywords.Should().Be("class");
        result.Namespace.Should().BeNull();
        result.IsNamespace.Should().BeFalse();

        result.Parent.IsNamespace.Should().BeTrue();
        result.Parent.Namespace.Should().BeNull();
        result.Parent.Name.Should().Be("");
        result.Parent.Parent.Should().Be(default(TypeHierarchyInfo));
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
        var symbol = GetTypeSymbol(code, "Inner");

        var result = TypeHierarchyInfo.Create(symbol);

        result.Name.Should().Be("Inner");
        result.Keywords.Should().Be("class");
        result.Namespace.Should().Be("NS");
        result.IsNamespace.Should().BeFalse();

        result.Parent.Name.Should().Be("Outer");
        result.Parent.Keywords.Should().Be("class");
        result.Parent.Namespace.Should().Be("NS");
        result.Parent.IsNamespace.Should().BeFalse();

        result.Parent.Parent.Name.Should().Be("NS");
        result.Parent.Parent.Keywords.Should().Be("namespace");
        result.Parent.Parent.IsNamespace.Should().BeTrue();

        result.Parent.Parent.Parent.Should().Be(default(TypeHierarchyInfo));
    }

    [Fact]
    public void Create_RecordClass_ReturnsCorrectKeyword()
    {
        const string code = @"
namespace NS
{
    record class MyRecord { }
}";
        var symbol = GetTypeSymbol(code, "MyRecord");

        var result = TypeHierarchyInfo.Create(symbol);

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
        var symbol = GetTypeSymbol(code, "MyRecordStruct");

        var result = TypeHierarchyInfo.Create(symbol);

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
        var symbol = GetTypeSymbol(code, "MyStruct");

        var result = TypeHierarchyInfo.Create(symbol);

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
        var symbol = GetTypeSymbol(code, "IMyInterface");

        var result = TypeHierarchyInfo.Create(symbol);

        result.Name.Should().Be("IMyInterface");
        result.Keywords.Should().Be("interface");
        result.IsNamespace.Should().BeFalse();
    }

    [Fact]
    public void Create_Enum_ReturnsCorrectKeyword()
    {
        const string code = @"
namespace NS
{
    enum MyEnum { A, B }
}";
        var symbol = GetTypeSymbol(code, "MyEnum");

        var result = TypeHierarchyInfo.Create(symbol);

        result.Name.Should().Be("MyEnum");
        result.Keywords.Should().Be("enum");
        result.IsNamespace.Should().BeFalse();
    }

    [Fact]
    public void Create_GenericClass_TypeParametersOnlyInName()
    {
        const string code = @"
namespace NS
{
    class Generic<T, U> { }
}";
        var symbol = GetTypeSymbol(code, "Generic");

        var result = TypeHierarchyInfo.Create(symbol);

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
        var symbol = GetTypeSymbol(code, "Inner");

        var result = TypeHierarchyInfo.Create(symbol);

        result.Name.Should().Be("Inner");
        result.Keywords.Should().Be("record struct");
        result.Namespace.Should().Be("NS");
        result.IsNamespace.Should().BeFalse();

        result.Parent.Name.Should().Be("Outer");
        result.Parent.Keywords.Should().Be("class");
        result.Parent.Namespace.Should().Be("NS");
        result.Parent.IsNamespace.Should().BeFalse();

        result.Parent.Parent.Name.Should().Be("NS");
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
        var symbol = GetTypeSymbol(code, "L3");

        var result = TypeHierarchyInfo.Create(symbol);

        result.Name.Should().Be("L3");
        result.Keywords.Should().Be("struct");
        result.IsNamespace.Should().BeFalse();

        result.Parent.Name.Should().Be("L2");
        result.Parent.Keywords.Should().Be("class");
        result.Parent.IsNamespace.Should().BeFalse();

        result.Parent.Parent.Name.Should().Be("L1");
        result.Parent.Parent.Keywords.Should().Be("class");
        result.Parent.Parent.IsNamespace.Should().BeFalse();

        result.Parent.Parent.Parent.Name.Should().Be("A.B.C");
        result.Parent.Parent.Parent.Keywords.Should().Be("namespace");
        result.Parent.Parent.Parent.IsNamespace.Should().BeTrue();

        result.Parent.Parent.Parent.Parent.Should().Be(default(TypeHierarchyInfo));
    }

    [Fact]
    public void Create_NestedGenericClass_TypeParametersOnlyInName()
    {
        const string code = @"
namespace NS
{
    class Outer<T>
    {
        class Inner<U> { }
    }
}";
        var symbol = GetTypeSymbol(code, "Inner");

        var result = TypeHierarchyInfo.Create(symbol);

        result.Name.Should().Be("Inner<U>");
        result.Keywords.Should().Be("class");
        result.Parent.Name.Should().Be("Outer<T>");
        result.Parent.Keywords.Should().Be("class");
    }

    [Fact]
    public void Create_RecordWithoutClassOrStructKeyword_KeywordIsRecordClass()
    {
        const string code = @"
namespace NS
{
    record MyRecord { }
}";
        var symbol = GetTypeSymbol(code, "MyRecord");

        var result = TypeHierarchyInfo.Create(symbol);

        result.Name.Should().Be("MyRecord");
        result.Keywords.Should().Be("record class");
    }

    [Fact]
    public void Create_TypesProperty_ContainsCorrectTypeNodes()
    {
        const string code = @"
namespace NS
{
    class Outer
    {
        record struct Inner(int X);
    }
}";
        var symbol = GetTypeSymbol(code, "Inner");

        var result = TypeHierarchyInfo.Create(symbol);

        result.Types.Length.Should().Be(2);
        result.Types.Span[0].Should().Be(new TypeHierarchyInfo.TypeNode("class", "Outer"));
        result.Types.Span[1].Should().Be(new TypeHierarchyInfo.TypeNode("record struct", "Inner"));
    }

    [Fact]
    public void Create_TypesProperty_SingleTypeHasOneNode()
    {
        const string code = @"
namespace NS
{
    struct MyStruct { }
}";
        var symbol = GetTypeSymbol(code, "MyStruct");

        var result = TypeHierarchyInfo.Create(symbol);

        result.Types.Length.Should().Be(1);
        result.Types.Span[0].Should().Be(new TypeHierarchyInfo.TypeNode("struct", "MyStruct"));
    }

    [Fact]
    public void Create_GenericRecordStruct_TypeParametersOnlyInName()
    {
        const string code = @"
namespace NS
{
    record struct Wrapper<T>(T Value);
}";
        var symbol = GetTypeSymbol(code, "Wrapper");

        var result = TypeHierarchyInfo.Create(symbol);

        result.Name.Should().Be("Wrapper<T>");
        result.Keywords.Should().Be("record struct");
        result.Namespace.Should().Be("NS");
    }

    [Fact]
    public void Create_RefStruct_ReturnsStructKeyword()
    {
        const string code = @"
namespace NS
{
    ref struct MyRefStruct { }
}";
        var symbol = GetTypeSymbol(code, "MyRefStruct");

        var result = TypeHierarchyInfo.Create(symbol);

        result.Name.Should().Be("MyRefStruct");
        result.Keywords.Should().Be("struct");
        result.IsNamespace.Should().BeFalse();
    }
}