using FluentAssertions;
using Trarizon.Library.Functional;

namespace Trarizon.Test.UnitTests.Functional;

public class OptionalTests
{
    [Fact]
    public void DefaultOrNonParamCtorCreation()
    {
        new Optional<int>().HasValue.Should().BeFalse();
        default(Optional<int>).HasValue.Should().BeFalse();
    }

    [Fact]
    public void FlatternTest()
    {
        var opt = Optional.Of(Optional.Of(1));
        opt.Value.HasValue.Should().BeTrue();
        
        opt = Optional.Of(default(Optional<int>));
        opt.Value.HasValue.Should().BeFalse();

        opt = default(Optional<Optional<int>>);
        opt.Value.HasValue.Should().BeFalse();
    }
}
