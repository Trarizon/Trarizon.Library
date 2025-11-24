using FluentAssertions;
using System.Runtime.CompilerServices;
using Trarizon.Library.Functional.Internal;

namespace Trarizon.Test.UnitTests.Functional;

public class BoxHelpersTests
{
    [Fact]
    public void NullBoxTest()
    {
        object? obj = null, obj2 = null;
        BoxHelpers.Box(obj).Should().BeNull();
        BoxHelpers.Unbox<object>(null).Should().BeNull();
        BoxHelpers.Unbox<int>(null).Should().Be(0);
        var box1 = BoxHelpers.Box(0);
        box1.Should().Match(obj => BoxHelpers.IsValidBox<int>(obj));
        Assert.RefEqual(in BoxHelpers.UnboxRef<int>(in obj), in BoxHelpers.UnboxRef<int>(in box1));
        var box2 = BoxHelpers.Box(false);
        box2.Should().Match(obj => BoxHelpers.IsValidBox<bool>(obj));
        Assert.RefEqual(in BoxHelpers.UnboxRef<bool>(in obj), in BoxHelpers.UnboxRef<bool>(in box2));
        var box3 = BoxHelpers.Box(default(int?));
        box3.Should().Match(obj => BoxHelpers.IsValidBox<int?>(obj));
        Assert.RefEqual(in BoxHelpers.UnboxRef<int?>(in obj), in BoxHelpers.UnboxRef<int?>(in box3));
        var box4 = BoxHelpers.Box(default(DateTime));
        box4.Should().Match(obj => BoxHelpers.IsValidBox<DateTime>(obj));
        Assert.RefEqual(in BoxHelpers.UnboxRef<DateTime>(in obj), in BoxHelpers.UnboxRef<DateTime>(in box4));
        Assert.RefEqual(in BoxHelpers.UnboxRef<(int, string)>(in obj), in BoxHelpers.UnboxRef<(int, string)>(in obj2));
    }

    [Fact]
    public void ReferenceTypeBoxTest()
    {
        var obj = new object();
        BoxHelpers.Box(obj).Should().BeOfType<object>()
            .And.Match(obj => BoxHelpers.IsValidBox<object>(obj))
            .And.Be(obj);
        var tpl = new int[1];
        BoxHelpers.Box(tpl).Should().BeOfType<int[]>()
            .And.Match(obj => BoxHelpers.IsValidBox<int[]>(obj))
            .And.Be(tpl);
    }

    [Fact]
    public void SpecialValueTypeBoxText()
    {
        var box1 = BoxHelpers.Box(1);
        box1.Should().BeOfType<ValueBox<int>>()
            .And.Match(obj => BoxHelpers.IsValidBox<int>(obj))
            .And.Subject.As<ValueBox<int>>().Value.Should().Be(1);
        BoxHelpers.Unbox<int>(box1).Should().Be(1);
        var box2 = BoxHelpers.Box(1);
        box2.Should().BeOfType<ValueBox<int>>()
            .And.Match(obj => BoxHelpers.IsValidBox<int>(obj))
            .And.Be(box1);


        var box3 = BoxHelpers.Box(30);
        box3.Should().BeOfType<ValueBox<int>>()
            .And.Match(obj => BoxHelpers.IsValidBox<int>(obj))
            .And.Subject.As<ValueBox<int>>().Value.Should().Be(30);
        BoxHelpers.Unbox<int>(box3).Should().Be(30);
        var box4 = BoxHelpers.Box(30);
        box4.Should().BeOfType<ValueBox<int>>()
            .And.Match(obj => BoxHelpers.IsValidBox<int>(obj))
            .And.NotBe(box3);
        BoxHelpers.Unbox<int>(box4).Should().Be(30);

        BoxHelpers.Box(true).Should().BeOfType<ValueBox<bool>>()
            .And.Match(obj => BoxHelpers.IsValidBox<bool>(obj))
            .And.Be(BoxHelpers.Box(true))
            .And.Subject.As<ValueBox<bool>>().Value.Should().BeTrue();
        BoxHelpers.Unbox<bool>(BoxHelpers.Box(true)).Should().BeTrue();

        BoxHelpers.Box(false).Should().BeOfType<ValueBox<bool>>()
            .And.Match(obj => BoxHelpers.IsValidBox<bool>(obj))
            .And.Be(BoxHelpers.Box(false))
            .And.Subject.As<ValueBox<bool>>().Value.Should().BeFalse();
        BoxHelpers.Unbox<bool>(BoxHelpers.Box(false)).Should().BeFalse();
    }

    [Fact]
    public void ValueTypeBoxTest()
    {
        var v1 = (1, "str");
        var box1 = BoxHelpers.Box(v1);
        box1.Should().BeOfType<ValueBox<(int, string)>>();
        BoxHelpers.Box(v1).Should().BeOfType<ValueBox<(int, string)>>()
            .And.Match(obj => BoxHelpers.IsValidBox<(int, string)>(obj))
            .And.NotBe(box1);
        BoxHelpers.Unbox<(int, string)>(box1).Should().Be(v1);

        var box2 = BoxHelpers.Box(default(DateTime));
        box2.Should().BeOfType<ValueBox<long>>()
            .And.Match(obj => BoxHelpers.IsValidBox<DateTime>(obj))
            .And.Be(BoxHelpers.Box(default(DateTime)));
        BoxHelpers.Unbox<DateTime>(box2).Should().Be(default(DateTime));

        var box3 = BoxHelpers.Box(default(DateTime?));
        box3.Should().BeOfType<ValueBox<DateTime?>>()
            .And.Match(obj => BoxHelpers.IsValidBox<DateTime?>(obj))
            .And.Be(BoxHelpers.Box(default(DateTime?)));
        BoxHelpers.Unbox<DateTime?>(box3).Should().Be(default(DateTime?));
    }
}
