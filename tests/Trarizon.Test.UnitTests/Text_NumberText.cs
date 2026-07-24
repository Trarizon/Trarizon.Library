using FluentAssertions;
using Trarizon.Library.Text;

namespace Trarizon.Test.UnitTests;

public class Text_NumberTextTests
{
    [Fact]
    public void NumberToChineseTests()
    {
        NumberText.NumberToChinese(1).Should().Be("一");
        NumberText.NumberToChinese(10).Should().Be("十");
        NumberText.NumberToChinese(100).Should().Be("一百");
        NumberText.NumberToChinese(1000).Should().Be("一千");
        NumberText.NumberToChinese(1_0000).Should().Be("一万");
        NumberText.NumberToChinese(10_0000).Should().Be("十万");
        NumberText.NumberToChinese(100_0000).Should().Be("一百万");
        NumberText.NumberToChinese(1000_0000).Should().Be("一千万");
        NumberText.NumberToChinese(1_0000_0000).Should().Be("一亿");
        NumberText.NumberToChinese(10_0000_0000).Should().Be("十亿");

        NumberText.NumberToChinese(101).Should().Be("一百零一");
        NumberText.NumberToChinese(1001).Should().Be("一千零一");
        NumberText.NumberToChinese(1_0001).Should().Be("一万零一");
        NumberText.NumberToChinese(10_0001).Should().Be("十万零一");
        NumberText.NumberToChinese(1_0000_0001).Should().Be("一亿零一");

        NumberText.NumberToChinese(1_1000).Should().Be("一万一千");
        NumberText.NumberToChinese(10_1000).Should().Be("十万一千");
        NumberText.NumberToChinese(10_0100).Should().Be("十万零一百");
        NumberText.NumberToChinese(1_0001_0000).Should().Be("一亿零一万");

        NumberText.NumberToChinese(1011).Should().Be("一千零十一");
        NumberText.NumberToChinese(1101).Should().Be("一千一百零一");
        NumberText.NumberToChinese(10101).Should().Be("一万零一百零一");
        NumberText.NumberToChinese(1_0001_0001).Should().Be("一亿零一万零一");
    }
}